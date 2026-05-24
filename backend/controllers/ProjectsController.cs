using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Branch Manager")]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AssistanceManagementSystem.Models.ApplicationUser> _userManager;

        public ProjectsController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<AssistanceManagementSystem.Models.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString, int? branchFilter)
        {
            var projects = _context.Projects.Include(p => p.Branch).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                projects = projects.Where(p => p.Name.Contains(searchString) || (p.Type ?? "").Contains(searchString) || (p.Address ?? "").Contains(searchString));
            }

            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            if (branchFilter.HasValue && User.IsInRole("Admin"))
            {
                projects = projects.Where(p => p.BranchId == branchFilter.Value);
                ViewBag.BranchFilter = branchFilter.Value;
            }

            ViewBag.SearchString = searchString;
            return View(await projects.OrderBy(p => p.Name).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.Include(p => p.Branch).FirstOrDefaultAsync(m => m.Id == id);
            if (project == null) return NotFound();

            return View(project);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Type,Address,Phone,BranchId")] Project project)
        {
            if (ModelState.IsValid)
            {
                // If creator is Branch Manager (not Admin) and branch not provided, assign their branch
                if (!User.IsInRole("Admin"))
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser?.BranchId != null)
                        project.BranchId = currentUser.BranchId;
                }

                _context.Add(project);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة المشروع بنجاح";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            return View(project);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,Address,Phone,BranchId")] Project project)
        {
            if (id != project.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Only allow changing branch if Admin
                    if (!User.IsInRole("Admin"))
                    {
                        var existing = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        project.BranchId = existing?.BranchId;
                    }
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تحديث المشروع بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FirstOrDefaultAsync(m => m.Id == id);
            if (project == null) return NotFound();

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف المشروع بنجاح";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ExportCsv(int? branchFilter)
        {
            var projects = _context.Projects.Include(p => p.Branch).AsQueryable();
            if (branchFilter.HasValue)
                projects = projects.Where(p => p.BranchId == branchFilter.Value);

            var list = await projects.ToListAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Id,Name,Type,Address,Phone,Branch");
            foreach (var p in list)
            {
                var line = $"{p.Id},\"{p.Name}\",\"{p.Type}\",\"{p.Address}\",{p.Phone ?? ""},\"{p.Branch?.Name ?? ""}\"";
                sb.AppendLine(line);
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"projects_{DateTime.Now:yyyyMMdd}.csv");
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
