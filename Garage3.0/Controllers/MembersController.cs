using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Garage3.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NuGet.Packaging.Rules;

namespace Garage3.Controllers
{
    public class MembersController : Controller
    {
        private readonly GarageContext _context;

        public MembersController(GarageContext context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index()
        {
           
            return View(await _context.Members.Include(p => p.Vehicles).OrderBy(p => EF.Functions.Collate(p.FirstName.Substring(0, 2), "Latin1_General_BIN")).ToListAsync());
        }


        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Added Include p.Vehicles to connect the Vehicles with Member
            var member = await _context.Members
                .Include(p => p.Vehicles)
                .ThenInclude(v => v.VehicleType) // Include VehicleType for each Vehicle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member member)
        {
            if (ModelState.IsValid)
            {
                if (member.FirstName == member.LastName)
                {
                    ModelState.AddModelError(string.Empty, "Förnamnet och efternamnet kan inte vara samma.");
                    return View(member);
                }
                else if (!IsValidIdFormat(member.PersonalIdentificationNumber, ModelState))
                {
                    ModelState.AddModelError("PersonalIdentificationNumber", "Bad format on Personal Id Number.");
                }
                else if (_context.Members.Any(m => m.PersonalIdentificationNumber == member.PersonalIdentificationNumber))
                {
                    ModelState.AddModelError("PersonalIdentificationNumber", "Personal number must be unique");
                }
                else
                {
                    _context.Add(member);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(member);
        }

        private bool IsValidIdFormat(Member member)
        {
            var id = member.PersonalIdentificationNumber;
            int index = id.IndexOf('-');
            string date;
            string last;

            if (id.Length > 11)
            {
                date = id.Substring(0, 8);
                last = id.Substring(8);
            }
            else
            {
                date = id.Substring(0, 6);
                last = id.Substring(6);
            }

            if (index > 0)
            {
                last = last.Substring(1);
            }

            int year;
            if (!int.TryParse(date.Substring(0, date.Length - 4), out year)) return false;

            int month;
            if (!int.TryParse(date.Substring(date.Length - 4, 2), out month)) return false;

            int day;
            if (!int.TryParse(date.Substring(date.Length - 2, 2), out day)) return false;

            var d = new DateOnly(year, month, day);

            return true;
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,DateOfBirth,PersonalIdentificationNumber")] Member member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
            
                 
                 if(member.FirstName == member.LastName )
                {
                    ModelState.AddModelError(string.Empty, "Förnamnet och efternamnet kan inte vara samma.");
                    return View(member);
                }
                
                else
                {
                   
               
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id))
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
            return View(member);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }


            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }
    }
}
