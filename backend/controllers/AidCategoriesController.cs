using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Branch Manager")]
    public class AidCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AidCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.AidCategories
                .Include(c => c.ParentAidCategory)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.NameAr)
                .ToListAsync();

            return View(categories);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.AidCategories
                .Include(c => c.ParentAidCategory)
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        public async Task<IActionResult> Create()
        {
            await LoadParentCategoriesAsync();
            return View(new AidCategory
            {
                IsActive = true,
                Scope = "Both"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NameAr,NameEn,Scope,Code,Description,IsActive,AllowCustomText,SortOrder,ParentAidCategoryId")] AidCategory aidCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aidCategory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة التصنيف بنجاح";
                return RedirectToAction(nameof(Index));
            }

            await LoadParentCategoriesAsync(aidCategory.ParentAidCategoryId);
            return View(aidCategory);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.AidCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            await LoadParentCategoriesAsync(category.ParentAidCategoryId, category.Id);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NameAr,NameEn,Scope,Code,Description,IsActive,AllowCustomText,SortOrder,ParentAidCategoryId")] AidCategory aidCategory)
        {
            if (id != aidCategory.Id)
            {
                return NotFound();
            }

            if (aidCategory.ParentAidCategoryId.HasValue && aidCategory.ParentAidCategoryId.Value == aidCategory.Id)
            {
                ModelState.AddModelError(nameof(aidCategory.ParentAidCategoryId), "لا يمكن ربط التصنيف بنفسه كتصنيف أب.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aidCategory);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تحديث التصنيف بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AidCategoryExists(aidCategory.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await LoadParentCategoriesAsync(aidCategory.ParentAidCategoryId, aidCategory.Id);
            return View(aidCategory);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.AidCategories
                .Include(c => c.ParentAidCategory)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _context.AidCategories.AnyAsync(c => c.ParentAidCategoryId == id))
            {
                TempData["ErrorMessage"] = "لا يمكن حذف التصنيف لأنه يحتوي على تصنيفات فرعية.";
                return RedirectToAction(nameof(Index));
            }

            var category = await _context.AidCategories.FindAsync(id);
            if (category != null)
            {
                _context.AidCategories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف التصنيف بنجاح";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadParentCategoriesAsync(int? selectedParentId = null, int? excludedCategoryId = null)
        {
            var query = _context.AidCategories
                .AsNoTracking()
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.NameAr)
                .AsQueryable();

            if (excludedCategoryId.HasValue)
            {
                query = query.Where(c => c.Id != excludedCategoryId.Value);
            }

            var categories = await query.ToListAsync();
            ViewBag.ParentCategories = new SelectList(categories, "Id", "NameAr", selectedParentId);
        }

        private bool AidCategoryExists(int id)
        {
            return _context.AidCategories.Any(e => e.Id == id);
        }
    }
}