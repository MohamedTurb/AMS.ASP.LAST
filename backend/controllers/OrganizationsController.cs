using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Branch Manager")]
    public class OrganizationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AssistanceManagementSystem.Models.ApplicationUser> _userManager;

        public OrganizationsController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<AssistanceManagementSystem.Models.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString, int? branchFilter)
        {
            var organizations = _context.Organizations.Include(o => o.Branch).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                organizations = organizations.Where(o => o.Name.Contains(searchString) || (o.Type ?? "").Contains(searchString) || (o.Address ?? "").Contains(searchString));
            }

            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            if (branchFilter.HasValue && User.IsInRole("Admin"))
            {
                organizations = organizations.Where(o => o.BranchId == branchFilter.Value);
                ViewBag.BranchFilter = branchFilter.Value;
            }

            ViewBag.SearchString = searchString;
            return View(await organizations.OrderBy(o => o.Name).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var organization = await _context.Organizations.Include(o => o.Branch).FirstOrDefaultAsync(m => m.Id == id);
            if (organization == null) return NotFound();

            return View(organization);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Type,Address,Phone,AccountNumber,BranchId")] Organization organization)
        {
            if (ModelState.IsValid)
            {
                if (!User.IsInRole("Admin"))
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser?.BranchId != null)
                        organization.BranchId = currentUser.BranchId;
                }

                _context.Add(organization);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة المنظمة بنجاح";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            return View(organization);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null) return NotFound();
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            return View(organization);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,Address,Phone,AccountNumber,BranchId")] Organization organization)
        {
            if (id != organization.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (!User.IsInRole("Admin"))
                    {
                        var existing = await _context.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
                        organization.BranchId = existing?.BranchId;
                    }
                    _context.Update(organization);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تحديث المنظمة بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationExists(organization.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(organization);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var organization = await _context.Organizations.FirstOrDefaultAsync(m => m.Id == id);
            if (organization == null) return NotFound();

            return View(organization);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization != null)
            {
                _context.Organizations.Remove(organization);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف المنظمة بنجاح";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ExportCsv(int? branchFilter)
        {
            var orgs = _context.Organizations.Include(o => o.Branch).AsQueryable();
            if (branchFilter.HasValue)
                orgs = orgs.Where(o => o.BranchId == branchFilter.Value);

            var list = await orgs.ToListAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Id,Name,Type,Address,Phone,AccountNumber,Branch");
            foreach (var o in list)
            {
                var line = $"{o.Id},\"{o.Name}\",\"{o.Type}\",\"{o.Address}\",{o.Phone ?? ""},\"{o.AccountNumber}\",\"{o.Branch?.Name ?? ""}\"";
                sb.AppendLine(line);
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"organizations_{DateTime.Now:yyyyMMdd}.csv");
        }

        private bool OrganizationExists(int id)
        {
            return _context.Organizations.Any(e => e.Id == id);
        }
    }
}
