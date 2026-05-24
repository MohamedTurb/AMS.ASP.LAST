using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BranchesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BranchesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var branches = await _context.Branches
                .OrderBy(b => b.Name)
                .ToListAsync();

            return View(branches);
        }

        [Authorize(Roles = "Admin,Branch Manager")]
        public async Task<IActionResult> Dashboard(int? id)
        {
            var branchId = id;
            if (branchId == null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                branchId = currentUser?.BranchId;
            }

            if (branchId == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == branchId.Value);
            if (branch == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Branch Manager"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.BranchId != branch.Id)
                {
                    return Forbid();
                }
            }

            var requests = _context.AssistanceRequests.Where(ar => ar.BranchId == branch.Id);

            ViewBag.BranchName = branch.Name;
            ViewBag.TotalRequests = await requests.CountAsync();
            ViewBag.PendingRequests = await requests.CountAsync(ar => ar.Status == "معلق");
            ViewBag.ApprovedRequests = await requests.CountAsync(ar => ar.Status == "موافق عليه");
            ViewBag.RejectedRequests = await requests.CountAsync(ar => ar.Status == "مرفوض");
            ViewBag.ExecutedRequests = await requests.CountAsync(ar => ar.Status == "تم التنفيذ");
            ViewBag.LastMonthRequests = await requests.CountAsync(ar => ar.CreatedAt >= DateTime.Today.AddDays(-30));
            ViewBag.StaffCount = await _context.Users.CountAsync(u => u.BranchId == branch.Id && u.UserName != null);
            ViewBag.BeneficiaryCount = await _context.Beneficiaries.CountAsync(b => b.BranchId == branch.Id);

            return View(branch);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == id);
            if (branch == null) return NotFound();

            ViewBag.Staff = await _context.Users
                .Where(u => u.BranchId == branch.Id)
                .OrderBy(u => u.FullName)
                .ToListAsync();
            ViewBag.Beneficiaries = await _context.Beneficiaries
                .Where(b => b.BranchId == branch.Id)
                .OrderBy(b => b.FullName)
                .ToListAsync();

            return View(branch);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,Phone")] Branch branch)
        {
            if (!ModelState.IsValid)
            {
                return View(branch);
            }

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم إضافة الفرع بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound();

            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Phone")] Branch branch)
        {
            if (id != branch.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(branch);
            }

            try
            {
                _context.Update(branch);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث الفرع بنجاح";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BranchExists(branch.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == id);
            if (branch == null) return NotFound();

            return View(branch);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch != null)
            {
                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الفرع بنجاح";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BranchExists(int id)
        {
            return _context.Branches.Any(e => e.Id == id);
        }
    }
}
