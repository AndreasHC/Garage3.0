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
            ViewBag.GarageIsFull = GarageIsFull();
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
        public IActionResult Create()
        {
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name");
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
            if (GarageIsFull())
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
                int spot = -1;
                if (spotSizeIsInverted)
                {
                    // Try to find a spot occupied by vehicles of the same size, but not to capacity
                    var  spotsOccupiedByRightSize = await _context.SpotOccupations.Include(o => o.Vehicle).ThenInclude(v => v.VehicleType).Where(o => o.Vehicle.VehicleType.SizeIsInverted && o.Vehicle.VehicleType.Size == spotSize).Select(o => o.SpotId).ToListAsync();
                    foreach (int spotToConsider in spotsOccupiedByRightSize)
                    {
                        if (await _context.SpotOccupations.Where(o => o.SpotId == spotToConsider).CountAsync() < spotSize)
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
                    for (int i = 0; i < _configuration.GetValue<int>("AppSettings:NumberOfSpots") - spotSize + 1; i++)
                    {
                        if (!await _context.SpotOccupations.Where(s => (s.SpotId >= i) & (s.SpotId < i + spotSize)).AnyAsync())
                        {
                            spot = i; break;
                        }
                    }
                }
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
                if (_context.Vehicles.Any(m => m.Id != vehicle.Id && m.RegistrationNumber != vehicle.RegistrationNumber))
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

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }

        private bool GarageIsFull()
        {
            return _context.Vehicles.Count() >= _configuration.GetValue<int>("AppSettings:NumberOfSpots");
        }
    }
}
