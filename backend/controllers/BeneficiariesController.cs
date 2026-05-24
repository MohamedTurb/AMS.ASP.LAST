using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AssistanceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Branch Manager")]
    public class BeneficiariesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AssistanceManagementSystem.Models.ApplicationUser> _userManager;

        public BeneficiariesController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<AssistanceManagementSystem.Models.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString, int? branchFilter)
        {
            var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
            var beneficiaries = _context.Beneficiaries
                .Include(b => b.Branch)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                beneficiaries = beneficiaries.Where(b => b.FullName.Contains(searchString) || b.NationalId.Contains(searchString) || (b.Phone ?? "").Contains(searchString));
            }

            // Branch Manager sees only own branch
            if (User.IsInRole("Branch Manager"))
            {
                var userId = User?.Identity?.Name;
                // try to get branch id from claims if available
                var branchClaim = User?.Claims?.FirstOrDefault(c => c.Type == "branchId")?.Value;
                if (int.TryParse(branchClaim, out var bid))
                {
                    beneficiaries = beneficiaries.Where(b => b.BranchId == bid);
                }
            }

            if (branchFilter.HasValue && isAdmin)
            {
                beneficiaries = beneficiaries.Where(b => b.BranchId == branchFilter.Value);
                ViewBag.BranchFilter = branchFilter.Value;
            }

            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            ViewBag.SearchString = searchString;

            return View(await beneficiaries.OrderBy(b => b.FullName).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> ExportCsv(int? branchFilter)
        {
            var beneficiaries = _context.Beneficiaries.Include(b => b.Branch).AsQueryable();
            if (branchFilter.HasValue)
                beneficiaries = beneficiaries.Where(b => b.BranchId == branchFilter.Value);

            var list = await beneficiaries.ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,FullName,NationalId,Phone,Branch,CreatedAt");
            foreach (var b in list)
            {
                var line = $"{b.Id},\"{b.FullName}\",{b.NationalId},{b.Phone ?? ""},\"{b.Branch?.Name ?? ""}\",{b.CreatedAt:yyyy-MM-dd}";
                sb.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"beneficiaries_{DateTime.Now:yyyyMMdd}.csv");
        }

        // GET: Beneficiaries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var beneficiary = await _context.Beneficiaries.Include(b => b.Branch).FirstOrDefaultAsync(b => b.Id == id);
            if (beneficiary == null) return NotFound();

            return View(beneficiary);
        }

        // GET: Beneficiaries/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            return View();
        }

        // POST: Beneficiaries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,NationalId,Phone,Gender,Religion,MaritalStatus,FamilyMembers,Income,BranchId,AidCategoryId")] Beneficiary beneficiary)
        {
            if (ModelState.IsValid)
            {
                // Assign branch automatically for non-admin users
                if (!User.IsInRole("Admin"))
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser?.BranchId != null)
                        beneficiary.BranchId = currentUser.BranchId;
                }
                _context.Add(beneficiary);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إنشاء المستفيد بنجاح";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            return View(beneficiary);
        }

        // GET: Beneficiaries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var beneficiary = await _context.Beneficiaries.FindAsync(id);
            if (beneficiary == null) return NotFound();

            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            return View(beneficiary);
        }

        // POST: Beneficiaries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,NationalId,Phone,Gender,Religion,MaritalStatus,FamilyMembers,Income,BranchId,AidCategoryId")] Beneficiary beneficiary)
        {
            if (id != beneficiary.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Only Admin can change branch
                    if (!User.IsInRole("Admin"))
                    {
                        var existing = await _context.Beneficiaries.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                        beneficiary.BranchId = existing?.BranchId;
                    }
                    _context.Update(beneficiary);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تحديث بيانات المستفيد";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Beneficiaries.Any(b => b.Id == beneficiary.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            return View(beneficiary);
        }

        // GET: Beneficiaries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var beneficiary = await _context.Beneficiaries.Include(b => b.Branch).FirstOrDefaultAsync(b => b.Id == id);
            if (beneficiary == null) return NotFound();
            return View(beneficiary);
        }

        // POST: Beneficiaries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var beneficiary = await _context.Beneficiaries.FindAsync(id);
            if (beneficiary != null)
            {
                _context.Beneficiaries.Remove(beneficiary);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف المستفيد";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
