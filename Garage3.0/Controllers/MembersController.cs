using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Garage3.Data;
using Garage3.Helpers;

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
           var members = await _context.Members
                .Include(p => p.Vehicles!) // We claim that this does populate the Vehicles property with something, regardless of database state
                .Include(m => m.Membership)
                .OrderBy(p => EF.Functions.Collate(p.FirstName.Substring(0, 2), "Latin1_General_BIN")).ToListAsync()
 ;
            return View(members);
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
                .Include(p => p.Vehicles!) // We claim that this does populate the Vehicles property with something, regardless of database state
                .ThenInclude(v => v.VehicleType) // Include VehicleType for each Vehicle
                .Include(m => m.Membership)
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

                // Standardize representation of Personal Id Number
                if (member.PersonalIdentificationNumber.IndexOf('-') < 0)
                {
                    string id = member.PersonalIdentificationNumber;
                    string formatedId = $"{id.Substring(0, 8)}-{id.Substring(8)}";
                    member.PersonalIdentificationNumber = formatedId;
                }

                // Use Personal Id Number to set BirthDate
                DateTime? birthDate;
                try
                {
                    birthDate = MemberHelper.GetBirthDate(member.PersonalIdentificationNumber);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    // Use exception message as model error message for now
                    ModelState.AddModelError("PersonalIdentificationNumber", ex.Message);
                    return View(member);
                }

                if (birthDate is null)
                {
                    ModelState.AddModelError("PersonalIdentificationNumber", "Bad format on Personal Id Number.");
                }
                else if (birthDate >= DateTime.Today)
                {
                    ModelState.AddModelError("PersonalIdentificationNumber", "Creating people from the future doesn't work in this reality.");
                }
                else if (_context.Members.Any(m => m.PersonalIdentificationNumber == member.PersonalIdentificationNumber))
                {
                    ModelState.AddModelError("PersonalIdentificationNumber", "Personal number must be unique");
                }
                else
                {
                    _context.Add(member);
                    await _context.SaveChangesAsync();

                    var startDate = DateTime.Today;
                    var endDate = (birthDate?.AddYears(65) < DateTime.Today) ? startDate.AddDays(30) : startDate.AddYears(2);

                    _context.Add(new Membership
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        Type = MembershipType.Pro_Member,
                        MemberId = member.Id
                    });
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(member);
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
                //// Validate that personal number is not changed
                //var originalMember = await _context.Members.FindAsync(member.Id);
                //if (originalMember ==   )

                 
                 if(member.FirstName == member.LastName )
                {
                    ModelState.AddModelError(string.Empty, "Förnamnet och efternamnet kan inte vara samma.");
                    return View(member);
                }

                // Validate that the given Personal Id Number is unchanged. 
                // Find the member data from _context
                var originalMember = await _context.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == member.Id);

                if (originalMember is null ||
                    member.PersonalIdentificationNumber != originalMember.PersonalIdentificationNumber)
                {
                    ModelState.AddModelError(String.Empty, "You must not change the Personal Id Number.");
                    return View(originalMember);
                }

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
