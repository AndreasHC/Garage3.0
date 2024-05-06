using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Garage3.Data;

namespace Garage3.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly GarageContext _context;

        public VehiclesController(GarageContext context)
        {
            _context = context;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            var garageContext = _context.Vehicles.Include(v => v.VehicleType).Include(v=>v.Owner);
            return View(await garageContext.ToListAsync());
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

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name");
            ViewData["OwnerId"] = new SelectList(_context.Members.Where(member=> member.DateOfBirth.Date.AddYears(18) <= DateTime.Now.Date).Select(member => new { member.Id, Name = member.FirstName + " " + member.LastName }), "Id", "Name");
            return View();
        }

        // POST: Vehicles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RegistrationNumber,Color,Brand,VehicleTypeId,OwnerId")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                if (_context.Vehicles.Any(m => m.RegistrationNumber == vehicle.RegistrationNumber))
                {
                    ModelState.AddModelError("RegistrationNumber", "Registration number must be unique");
                    // Återställ ViewData för att behålla värdena för VehicleTypeId och OwnerId
                    ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
                    ViewData["OwnerId"] = new SelectList(_context.Members.Where(member => member.DateOfBirth.Date.AddYears(18) <= DateTime.Now.Date).Select(member => new { member.Id, Name = member.FirstName + " " + member.LastName }), "Id", "Name", vehicle.OwnerId);
                    return View(vehicle);
                }
                else
                {
                    vehicle.ParkingTime = DateTime.Now;
                    _context.Add(vehicle);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            // Återställ ViewData för att behålla värdena för VehicleTypeId och OwnerId
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
            ViewData["OwnerId"] = new SelectList(_context.Members.Where(member => member.DateOfBirth.Date.AddYears(18) <= DateTime.Now.Date).Select(member => new { member.Id, Name = member.FirstName + " " + member.LastName }), "Id", "Name", vehicle.OwnerId);
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
        
        public async Task<IActionResult> Edit(int id, [Bind("Id,RegistrationNumber,Color,Brand,ParkingTime,VehicleTypeId,OwnerId")] Vehicle vehicle)
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
                else
                {
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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
}
