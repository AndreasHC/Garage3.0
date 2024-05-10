using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Garage3.Data;
using Garage3.Helpers;
using Garage3.ViewModels;
using Garage3.Models;
using System.Text;

namespace Garage3.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly GarageContext _context;
        private readonly IConfiguration _configuration;

        public VehiclesController(GarageContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            ViewBag.VehicleTypeId = TypeFilterSelectList("All");
            ViewBag.GarageIsFull = await GarageIsFull();
            // Call the Spaces action to get occupancy data
            await Spaces();
            var garageContext = _context.Vehicles.Include(v => v.VehicleType).Include(v=>v.Owner);
            return View(await garageContext.ToListAsync());
        }


        // POST: Filtered vehicles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string soughtVehicleType, string soughtRegistrationNumber)
        {
            ViewBag.VehicleTypeId = TypeFilterSelectList(soughtVehicleType);
            IQueryable<Vehicle> garageContext = _context.Vehicles.Include(v => v.VehicleType).Include(v => v.Owner);
            if (soughtVehicleType != "All")
            {
                int soughtVehicleTypeInt = int.Parse(soughtVehicleType);
                garageContext = garageContext.Where(v => v.VehicleTypeId == soughtVehicleTypeInt);
            }
            if (!string.IsNullOrEmpty(soughtRegistrationNumber))
            {
                garageContext=garageContext.Where(v => v.RegistrationNumber.Contains(soughtRegistrationNumber));
                ViewBag.RegistrationNumber = soughtRegistrationNumber;
            }
            return View(await garageContext.ToListAsync());

        }

        private IEnumerable<SelectListItem> TypeFilterSelectList(string soughtVehicleType)
        {
            bool isAll = soughtVehicleType == "All";
            SelectList vehicleTypeSelectList;
            if (isAll)
                vehicleTypeSelectList = new SelectList(_context.VehicleTypes, "Id", "Name");
            else
                vehicleTypeSelectList = new SelectList(_context.VehicleTypes, "Id", "Name", soughtVehicleType);
            SelectListItem AllOption = new SelectListItem("All", "All");
            if (isAll)
                AllOption.Selected = true;
            return vehicleTypeSelectList.Prepend(AllOption);
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }
            var spots = await _context.SpotOccupations.Where(s => s.VehicleId == id).Select(s => s.SpotId).ToListAsync();
            ViewBag.SpotList = String.Join(", ", spots);

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public async Task<IActionResult> Create()
        {
            var viableVehicleTypes = await _context.VehicleTypes.ToAsyncEnumerable().WhereAwait(async (v) => (await FindSpot(v)) != -1).ToListAsync();
            ViewData["VehicleTypeId"] = new SelectList(viableVehicleTypes, "Id", "Name");
            ViewData["OwnerId"] = OwnerSelectList();
            return View();
        }

        private SelectList OwnerSelectList()
        {
            var members = _context.Members.ToList(); // This retrieves all members into memory
            return new SelectList(
                members
                .Where(member =>
                {
                    var bd = MemberHelper.GetBirthDate(member.PersonalIdentificationNumber);
                    return bd.HasValue && bd.Value.AddYears(18) <= DateTime.Now.Date;
                })
                .Select(member => new { member.Id, Name = member.FullName }), "Id", "Name");
        }

        // POST: Vehicles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RegistrationNumber,Color,Brand,VehicleTypeId,OwnerId")] Vehicle vehicle)
        {
            if (await GarageIsFull())
                 return View("~/Views/Vehicles/Reject.cshtml");
            if (ModelState.IsValid)
            {
                bool problemDetected = false;
                string errorKey = string.Empty;
                string errorValue = string.Empty;
                if (_context.Vehicles.Any(m => m.RegistrationNumber == vehicle.RegistrationNumber))
                {
                    problemDetected = true;
                    errorKey = "RegistrationNumber";
                    errorValue = "Registration number must be unique";
                }

                var owner = await _context.Members.FindAsync(vehicle.OwnerId);

                if (owner == null)
                {
                    problemDetected = true;
                    errorKey = "OwnerId";
                    errorValue = "Member must be registered";
                }

                else if (MemberHelper.GetBirthDate(owner.PersonalIdentificationNumber)?.AddYears(18) > DateTime.Today)
                {
                    problemDetected = true;
                    errorKey = "OwnerId";
                    errorValue = "Member must be at least 18 years old";
                }

                if (problemDetected)
                {
                    ModelState.AddModelError(errorKey, errorValue);
                    // Återställ ViewData för att behålla värdena för VehicleTypeId och OwnerId
                    ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
                    ViewData["OwnerId"] = OwnerSelectList();
                    return View(vehicle);
                }
                VehicleType vehicleType = await _context.VehicleTypes.FindAsync(vehicle.VehicleTypeId) ?? throw new InvalidDataException("Attempted to park vehicle of unregistered type.");
                int spotSize = vehicleType.Size;
                bool spotSizeIsInverted = vehicleType.SizeIsInverted;
                int spot = await FindSpot(vehicleType);
                if (spot == -1)
                    return View("~/Views/Vehicles/Reject.cshtml");

                vehicle.ParkingTime = DateTime.Now;
                _context.Add(vehicle);
                if (spotSizeIsInverted)
                {
                    _context.Add(new SpotOccupation() { SpotId = spot, VehicleId = vehicle.Id, Vehicle = vehicle });
                }
                else
                {
                    for (int i = 0; i < spotSize; i++)
                        _context.Add(new SpotOccupation() { SpotId = spot + i, VehicleId = vehicle.Id, Vehicle = vehicle });
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            // Återställ ViewData för att behålla värdena för VehicleTypeId och OwnerId
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
            ViewData["OwnerId"] = OwnerSelectList();
            return View(vehicle);
        }

        private async Task<int> FindSpot(VehicleType vehicleType)
        {
            int spot = -1;
            if (vehicleType.SizeIsInverted)
            {
                // Try to find a spot occupied by vehicles of the same size, but not to capacity
                var spotsOccupiedByRightSize = await _context.SpotOccupations.Include(o => o.Vehicle).ThenInclude(v => v.VehicleType).Where(o => o.Vehicle.VehicleType.SizeIsInverted && o.Vehicle.VehicleType.Size == vehicleType.Size).Select(o => o.SpotId).ToListAsync();
                foreach (int spotToConsider in spotsOccupiedByRightSize)
                {
                    if (await _context.SpotOccupations.Where(o => o.SpotId == spotToConsider).CountAsync() < vehicleType.Size)
                    {
                        spot = spotToConsider; break;
                    }
                }
                // Try for empty spot
                if (spot == -1)
                {
                    for (int i = 0; i < _configuration.GetValue<int>("AppSettings:NumberOfSpots"); i++)
                    {
                        if (!await _context.SpotOccupations.Where(s => s.SpotId == i).AnyAsync())
                        {
                            spot = i; break;
                        }
                    }
                }

            }

            else
            {
                for (int i = 0; i < _configuration.GetValue<int>("AppSettings:NumberOfSpots") - vehicleType.Size + 1; i++)
                {
                    if (!await _context.SpotOccupations.Where(s => (s.SpotId >= i) & (s.SpotId < i + vehicleType.Size)).AnyAsync())
                    {
                        spot = i; break;
                    }
                }
            }

            return spot;
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            var selectedVehicleType = await _context.VehicleTypes.FindAsync(vehicle.VehicleTypeId);
            if (selectedVehicleType != null)
            {
                ViewBag.VehicleTypeName = selectedVehicleType.Name;
            }
            else
            {
                ViewBag.VehicleTypeName = ""; // Om det inte finns någon matchande VehicleType, sätt VehicleTypeName till en tom sträng
            }

            ViewData["OwnerId"] = vehicle.OwnerId;
            return View(vehicle);
        }

      

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RegistrationNumber,Color,Brand,ParkingTime,VehicleTypeId, OwnerId")] Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (_context.Vehicles.Any(m => m.Id != vehicle.Id && m.RegistrationNumber == vehicle.RegistrationNumber))
                {
                    ModelState.AddModelError("RegistrationNumber", "Registration number must be unique.");
                    ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);

                    ViewBag.OwnerId = vehicle.OwnerId;

                    // Hämta och sätt VehicleTypeName igen
                    ViewBag.VehicleTypeName = await _context.VehicleTypes
                        .Where(vt => vt.Id == vehicle.VehicleTypeId)
                        .Select(vt => vt.Name)
                        .FirstOrDefaultAsync();

                    return View(vehicle);
                }

                try
                {
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
            ViewBag.OwnerId = vehicle.OwnerId;
            return View(vehicle);
        }
        // GET: Vehicles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Receipt), vehicle);
            }

            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Receipt(Vehicle vehicle)
        {
            Member owner = await _context.Members.FindAsync(vehicle.OwnerId) ?? throw new InvalidDataException("Tried to generate receipt without registered owner");
            string parkerName = owner.FullName;
            Receipt receipt = new Receipt(vehicle, parkerName);
            return View(receipt);
        }

        //public async Task<IActionResult> Spaces()
        //{
        //    int numberOfSpots = _configuration.GetValue<int>("AppSettings:NumberOfSpots");
        //    Dictionary<int, bool> result = new Dictionary<int, bool>();
        //    //var spots =  _context.SpotOccupations.Include(a => a.Vehicle).Select(Vehicle);

        //    for (int i = 0; i < numberOfSpots; i++)
        //    {
        //        result[i] = await _context.SpotOccupations.Where(s => s.SpotId == i).AnyAsync();
        //    }
        //    ViewBag.OccupancyDict = result;
        //    ViewBag.NumberOfSpots = numberOfSpots;

        //    return View();
        //}

        public async Task<IActionResult> Spaces()
        {
            int numberOfSpots = _configuration.GetValue<int>("AppSettings:NumberOfSpots");
            Dictionary<int, bool> result = new Dictionary<int, bool>();
            Dictionary<int, List<string>> registrationNumbersDict = new Dictionary<int, List<string>>(); // Ändra till Dictionary

            for (int i = 0; i < numberOfSpots; i++)
            {
                bool isOccupied = await _context.SpotOccupations
                    .AnyAsync(s => s.SpotId == i && s.Vehicle != null);

                result[i] = isOccupied;

                if (isOccupied)
                {
                    // If spot is occupied, fetch the registration numbers and add them to the dictionary
                    var registrationNumbers = await _context.SpotOccupations
                        .Where(s => s.SpotId == i && s.Vehicle != null)
                        .Select(s => s.Vehicle.RegistrationNumber)
                        .ToListAsync(); // Hämta en lista med registreringsnummer för den aktuella platsen

                    registrationNumbersDict[i] = registrationNumbers; // Lägg till registreringsnummer i dictionary
                }
            }

            ViewBag.OccupancyDict = result;
            ViewBag.NumberOfSpots = numberOfSpots;
            ViewBag.RegistrationNumbers = registrationNumbersDict; // Uppdatera ViewBag.RegistrationNumbers till att använda dictionary

            // Pass a list of vehicles to the view
            var vehicles = await _context.Vehicles.ToListAsync();
            return View(vehicles);
        }




        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }

        

        // GET: Vehicles/Statistics
        [HttpGet]
        /*public async Task<IActionResult> Overview()
        {
            var vehicles = await _context.Vehicles.ToListAsync();

            var model = new VehiclesOverview
            {
                vehicleTypes = vehicles.Select(v => v.VehicleType).Distinct().Select(s => " " + s).ToList()
            };

            return View();
        }*/
        public async Task<IActionResult> Statistics()
        {
            var viewModel = new StatisticsViewModel
            {
                VehicleCountByType = await CalculateVehiclesCountByType(),
                TotalWheelsCount = await CalculateTotalWheelsCount(),
                TotalRevenue = await CalculateTotalRevenue(),
            };

            return View(viewModel);
        }

        private async Task<decimal> CalculateTotalRevenue()
        {
            decimal revenues = 0;
            DateTime now = DateTime.Now;

            await foreach (var vehicle in _context.Vehicles)
            {
                revenues += VehiclesHelper.GetParkingCost(vehicle.ParkingTime - now);
            }

            return revenues;
        }

        private async Task<int> CalculateTotalWheelsCount()
        {
            //  throw new NotImplementedException();
            int totalWheels = 0;

            // .Include(p => p.VehiclesTypes!)

            var vehicles = _context.Vehicles
                .Include(p => p.VehicleType!);
                

            foreach(var vehicle in vehicles)
            {
                totalWheels += vehicle.VehicleType.NumberOfWheels;
               // totalWheels = totalWheels + vehicle.VehicleType.NumberOfWheels;
            }

            return totalWheels;
        }

        private async Task<Dictionary<string, int>> CalculateVehiclesCountByType()
        {
            Dictionary<string,int> result = new();

            var vehicles = _context.Vehicles
                .Include(v => v.VehicleType)
                .Select(v => v)
                ;

            foreach (var vehicle in vehicles)
            {
                var type = vehicle.VehicleType.Name;
                if (result.ContainsKey(type))
                {
                    result[type]++;
                }
                else
                {
                    result[type] = 1;
                }
            }

            return result;
        }

        private async Task<bool> GarageIsFull()
        {
            int numberOfSpots = _configuration.GetValue<int>("AppSettings:NumberOfSpots");
            // find all small sizes currently in use
            var smallSizes = await _context.Vehicles.Include(v => v.VehicleType).Where(v => v.VehicleType.SizeIsInverted).GroupBy(v => v.VehicleType.Size).Select(group => group.Key).ToListAsync();
            int fullyOccupiedSpots = 0;
            foreach (int size in smallSizes)
            {
                // count (minimal) number of spots required to hold all small vehicles of this size (number of spots used may be larger)
                fullyOccupiedSpots += await _context.Vehicles.Include(v => v.VehicleType).Where(v => v.VehicleType.SizeIsInverted & v.VehicleType.Size == size).CountAsync() / size;
            }
            // count number of spots used to hold non-small vehicles
            fullyOccupiedSpots += await _context.Vehicles.Include(v => v.VehicleType).Where(v => !v.VehicleType.SizeIsInverted).SumAsync(v => v.VehicleType.Size);

            return fullyOccupiedSpots >= numberOfSpots;
        }
    }
}
