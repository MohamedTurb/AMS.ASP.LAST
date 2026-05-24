using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using AssistanceManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssistanceManagementSystem.Controllers
{
    public class AssistanceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAssistanceRequestFileService _fileService;

        public AssistanceRequestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IAssistanceRequestFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _fileService = fileService;
        }

        private async Task PopulateFormOptionsAsync()
        {
            var categories = await _context.AidCategories
                .AsNoTracking()
                .Include(c => c.ParentAidCategory)
                .Where(c => c.IsActive && (c.Scope == "Both" || c.Scope == "Request"))
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.NameAr)
                .ToListAsync();

            ViewBag.AssistanceTypes = categories.Select(category => new SelectListItem
            {
                Value = category.NameAr,
                Text = category.ParentAidCategoryId.HasValue && category.ParentAidCategory != null
                    ? $"{category.ParentAidCategory.NameAr} - {category.NameAr}"
                    : category.NameAr
            }).ToList();
            ViewBag.Genders = new List<string> { "ذكر", "أنثى" };
            ViewBag.Religions = new List<string> { "مسلم", "مسلمة", "مسيحي", "مسيحية", "أخرى" };
            ViewBag.MaritalStatuses = new List<string> { "أعزب", "عزباء", "متزوج", "متزوجة", "مطلق", "مطلقة", "أرمل", "أرملة" };
            ViewBag.Relationships = new List<string> { "أب", "أم", "ابن", "ابنة", "أخ", "أخت", "زوج", "زوجة", "جد", "جدة", "عم", "عمة", "خال", "خالة", "ابن عم", "ابنة عم", "ابن خال", "ابنة خال", "صديق", "صديقة", "جار", "جارة", "آخر" };
            // Branch list for forms
            var branches = await _context.Branches.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
            ViewBag.Branches = new SelectList(branches, "Id", "Name");
        }

        // GET: AssistanceRequests
        [Authorize]
        public async Task<IActionResult> Index(string searchString, string typeFilter, string statusFilter, DateTime? dateFrom, DateTime? dateTo, int? branchFilter)
        {
            var assistanceRequests = _context.AssistanceRequests
                .Include(ar => ar.CreatedByUser)
                .Include(ar => ar.ReviewedByUser)
                .Include(ar => ar.Branch)
                .AsQueryable();

            // Branch Manager can only see requests from their branch
            if (User.IsInRole("Branch Manager"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.BranchId != null)
                {
                    assistanceRequests = assistanceRequests.Where(ar => ar.BranchId == currentUser.BranchId);
                }
            }

            // Beneficiary can only see their own requests
            if (User.IsInRole("Beneficiary"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    assistanceRequests = assistanceRequests.Where(ar => ar.CreatedByUserId == currentUser.Id);
                }
            }

            // Admin branch filter
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            if (branchFilter.HasValue && User.IsInRole("Admin"))
            {
                assistanceRequests = assistanceRequests.Where(ar => ar.BranchId == branchFilter.Value);
                ViewBag.BranchFilter = branchFilter.Value;
            }

            ViewBag.TotalRequests = await assistanceRequests.CountAsync();
            ViewBag.PendingRequests = await assistanceRequests.CountAsync(ar => ar.Status == "معلق");
            ViewBag.ApprovedRequests = await assistanceRequests.CountAsync(ar => ar.Status == "موافق عليه");
            ViewBag.RejectedRequests = await assistanceRequests.CountAsync(ar => ar.Status == "مرفوض");
            ViewBag.ExecutedRequests = await assistanceRequests.CountAsync(ar => ar.Status == "تم التنفيذ");
            ViewBag.LastMonthRequests = await assistanceRequests.CountAsync(ar => ar.CreatedAt >= DateTime.Today.AddDays(-30));

            if (!string.IsNullOrEmpty(searchString))
            {
                assistanceRequests = assistanceRequests.Where(ar => 
                    (ar.RequesterName ?? "").Contains(searchString) ||
                    (ar.RequesterNationalId ?? "").Contains(searchString) ||
                    (ar.RequesterPhoneNumber ?? "").Contains(searchString) ||
                    (ar.RequesterAddress ?? "").Contains(searchString) ||
                    (ar.BeneficiaryName ?? "").Contains(searchString) ||
                    (ar.BeneficiaryNationalId ?? "").Contains(searchString) ||
                    (ar.BeneficiaryPhoneNumber ?? "").Contains(searchString) ||
                    (ar.BeneficiaryAddress ?? "").Contains(searchString) ||
                    (ar.Reason ?? "").Contains(searchString));
            }

            if (!string.IsNullOrEmpty(typeFilter))
            {
                assistanceRequests = assistanceRequests.Where(ar => ar.TypeOfAssistance == typeFilter);
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                assistanceRequests = assistanceRequests.Where(ar => ar.Status == statusFilter);
            }

            if (dateFrom.HasValue)
            {
                assistanceRequests = assistanceRequests.Where(ar => ar.CreatedAt >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                assistanceRequests = assistanceRequests.Where(ar => ar.CreatedAt <= dateTo.Value);
            }

            ViewBag.Types = await _context.AssistanceRequests.Select(ar => ar.TypeOfAssistance).Distinct().ToListAsync();
            ViewBag.Statuses = new List<string> { "معلق", "موافق عليه", "مرفوض", "تم التنفيذ" };

            return View(await assistanceRequests.OrderByDescending(ar => ar.CreatedAt).ToListAsync());
        }

        // GET: AssistanceRequests/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var assistanceRequest = await _context.AssistanceRequests
                .Include(ar => ar.CreatedByUser)
                .Include(ar => ar.ReviewedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (assistanceRequest == null) return NotFound();

            return View(assistanceRequest);
        }

        // GET: AssistanceRequests/Create
        public async Task<IActionResult> Create()
        {
            await PopulateFormOptionsAsync();
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                ViewBag.CurrentUserBranchId = currentUser?.BranchId;
            }
            return View();
        }

        // POST: AssistanceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequesterNameAr,RequesterNameEn,RequesterName,RequesterNationalId,RequesterPhoneNumber,RequesterAddress,BeneficiaryNameAr,BeneficiaryNameEn,BeneficiaryName,BeneficiaryNationalId,BeneficiaryPhoneNumber,BeneficiaryAddress,BeneficiaryGender,BeneficiaryReligion,BeneficiaryMaritalStatus,BeneficiaryFamilyMembers,BeneficiaryIncome,RelationshipToBeneficiary,TypeOfAssistance,AidCategoryDetails,Amount,Reason,SupportingDocuments,BranchId")] AssistanceRequest assistanceRequest, IFormFileCollection supportingFiles)
        {
            if (ModelState.IsValid)
            {
                var selectedCategory = await _context.AidCategories.FirstOrDefaultAsync(c => c.NameAr == assistanceRequest.TypeOfAssistance);
                assistanceRequest.AidCategoryId = selectedCategory?.Id;

                // Check if BeneficiaryNationalId already exists in Beneficiaries
                var existingBeneficiary = await _context.Beneficiaries
                    .FirstOrDefaultAsync(b => b.NationalId == assistanceRequest.BeneficiaryNationalId);
                
                if (existingBeneficiary != null)
                {
                    ModelState.AddModelError("BeneficiaryNationalId", "يوجد مستفيد مسجل بالفعل بنفس الرقم القومي");
                    await PopulateFormOptionsAsync();
                    return View(assistanceRequest);
                }

                // Check if RequesterNationalId already exists in AssistanceRequests
                var existingRequest = await _context.AssistanceRequests
                    .FirstOrDefaultAsync(ar => ar.RequesterNationalId == assistanceRequest.RequesterNationalId);
                
                if (existingRequest != null)
                {
                    ModelState.AddModelError("RequesterNationalId", "يوجد طلب مساعدة سابق بنفس الرقم القومي لطالب المساعدة");
                    await PopulateFormOptionsAsync();
                    return View(assistanceRequest);
                }

                // Ensure legacy name fields are populated from Arabic/English inputs for compatibility
                assistanceRequest.RequesterName = string.IsNullOrWhiteSpace(assistanceRequest.RequesterNameAr)
                    ? (assistanceRequest.RequesterNameEn ?? assistanceRequest.RequesterName)
                    : assistanceRequest.RequesterNameAr;

                assistanceRequest.BeneficiaryName = string.IsNullOrWhiteSpace(assistanceRequest.BeneficiaryNameAr)
                    ? (assistanceRequest.BeneficiaryNameEn ?? assistanceRequest.BeneficiaryName)
                    : assistanceRequest.BeneficiaryNameAr;

                // Create new Beneficiary
                var beneficiary = new Beneficiary
                {
                    FullNameAr = !string.IsNullOrWhiteSpace(assistanceRequest.BeneficiaryNameAr) ? assistanceRequest.BeneficiaryNameAr : null,
                    FullNameEn = !string.IsNullOrWhiteSpace(assistanceRequest.BeneficiaryNameEn) ? assistanceRequest.BeneficiaryNameEn : null,
                    FullName = !string.IsNullOrWhiteSpace(assistanceRequest.BeneficiaryNameAr) ? assistanceRequest.BeneficiaryNameAr : (assistanceRequest.BeneficiaryNameEn ?? assistanceRequest.BeneficiaryName),
                    NationalId = assistanceRequest.BeneficiaryNationalId,
                    Phone = assistanceRequest.BeneficiaryPhoneNumber,
                    Gender = assistanceRequest.BeneficiaryGender,
                    Religion = assistanceRequest.BeneficiaryReligion,
                    MaritalStatus = assistanceRequest.BeneficiaryMaritalStatus,
                    FamilyMembers = assistanceRequest.BeneficiaryFamilyMembers,
                    Income = assistanceRequest.BeneficiaryIncome,
                    AidCategoryId = selectedCategory?.Id,
                    BranchId = 1 // Default branch, can be updated later
                };

                _context.Beneficiaries.Add(beneficiary);
                await _context.SaveChangesAsync();

                // Handle file uploads
                if (supportingFiles != null && supportingFiles.Count > 0)
                {
                    var validationErrors = _fileService.Validate(supportingFiles);
                    if (validationErrors.Any())
                    {
                        foreach (var error in validationErrors)
                        {
                            ModelState.AddModelError("SupportingDocuments", error);
                        }

                        await PopulateFormOptionsAsync();
                        return View(assistanceRequest);
                    }

                    var fileNames = await _fileService.SaveFilesAsync(supportingFiles, _webHostEnvironment.WebRootPath);
                    assistanceRequest.SupportingDocuments = string.Join(",", fileNames);
                }

                // Create AssistanceRequest
                if (selectedCategory != null)
                {
                    assistanceRequest.TypeOfAssistance = selectedCategory.NameAr;
                }

                assistanceRequest.Status = "معلق";
                assistanceRequest.CreatedAt = DateTime.Now;
                
                // If user is logged in, set CreatedByUserId and BranchId
                ApplicationUser? currentUser = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    currentUser = await _userManager.GetUserAsync(User);
                    assistanceRequest.CreatedByUserId = _userManager.GetUserId(User);
                }

                // Associate the request with the appropriate branch (selected by user if provided, otherwise user's branch or beneficiary branch)
                if (!assistanceRequest.BranchId.HasValue)
                {
                    assistanceRequest.BranchId = currentUser?.BranchId ?? beneficiary.BranchId;
                }

                // Generate a unique reference number for the request (include branch when available)
                assistanceRequest.ReferenceNumber = await GenerateUniqueReferenceAsync(assistanceRequest.BranchId);

                _context.Add(assistanceRequest);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إرسال طلب المساعدة وإنشاء المستفيد بنجاح. سيتم مراجعته قريباً.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateFormOptionsAsync();
            return View(assistanceRequest);
        }

        // GET: AssistanceRequests/Edit/5
        [Authorize(Roles = "Admin,Branch Manager,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var assistanceRequest = await _context.AssistanceRequests.FindAsync(id);
            if (assistanceRequest == null) return NotFound();

            await PopulateFormOptionsAsync();
            ViewBag.Statuses = new List<string> { "معلق", "موافق عليه", "مرفوض", "تم التنفيذ" };

            return View(assistanceRequest);
        }

        // POST: AssistanceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Branch Manager,Staff")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RequesterNameAr,RequesterNameEn,RequesterName,RequesterNationalId,RequesterPhoneNumber,RequesterAddress,BeneficiaryNameAr,BeneficiaryNameEn,BeneficiaryName,BeneficiaryNationalId,BeneficiaryPhoneNumber,BeneficiaryAddress,BeneficiaryGender,BeneficiaryReligion,BeneficiaryMaritalStatus,BeneficiaryFamilyMembers,BeneficiaryIncome,RelationshipToBeneficiary,TypeOfAssistance,AidCategoryDetails,Amount,Reason,SupportingDocuments,Status,ReviewNotes,BranchId")] AssistanceRequest assistanceRequest)
        {
            if (id != assistanceRequest.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var selectedCategory = await _context.AidCategories.FirstOrDefaultAsync(c => c.NameAr == assistanceRequest.TypeOfAssistance);
                    var existingRequest = await _context.AssistanceRequests.FindAsync(id);
                    if (existingRequest == null) return NotFound();
                    var previousStatus = existingRequest.Status;

                    // Update fields
                    existingRequest.RequesterName = string.IsNullOrWhiteSpace(assistanceRequest.RequesterNameAr)
                        ? (assistanceRequest.RequesterNameEn ?? assistanceRequest.RequesterName)
                        : assistanceRequest.RequesterNameAr;
                    existingRequest.RequesterNationalId = assistanceRequest.RequesterNationalId;
                    existingRequest.RequesterPhoneNumber = assistanceRequest.RequesterPhoneNumber;
                    existingRequest.RequesterAddress = assistanceRequest.RequesterAddress;
                    existingRequest.BeneficiaryName = string.IsNullOrWhiteSpace(assistanceRequest.BeneficiaryNameAr)
                        ? (assistanceRequest.BeneficiaryNameEn ?? assistanceRequest.BeneficiaryName)
                        : assistanceRequest.BeneficiaryNameAr;
                    existingRequest.BeneficiaryNationalId = assistanceRequest.BeneficiaryNationalId;
                    existingRequest.BeneficiaryPhoneNumber = assistanceRequest.BeneficiaryPhoneNumber;
                    existingRequest.BeneficiaryAddress = assistanceRequest.BeneficiaryAddress;
                    existingRequest.BeneficiaryGender = assistanceRequest.BeneficiaryGender;
                    existingRequest.BeneficiaryReligion = assistanceRequest.BeneficiaryReligion;
                    existingRequest.BeneficiaryMaritalStatus = assistanceRequest.BeneficiaryMaritalStatus;
                    existingRequest.BeneficiaryFamilyMembers = assistanceRequest.BeneficiaryFamilyMembers;
                    existingRequest.BeneficiaryIncome = assistanceRequest.BeneficiaryIncome;
                    existingRequest.RelationshipToBeneficiary = assistanceRequest.RelationshipToBeneficiary;
                    existingRequest.TypeOfAssistance = assistanceRequest.TypeOfAssistance;
                    existingRequest.AidCategoryId = selectedCategory?.Id;
                    existingRequest.AidCategoryDetails = assistanceRequest.AidCategoryDetails;
                    existingRequest.Amount = assistanceRequest.Amount;
                    existingRequest.Reason = assistanceRequest.Reason;
                    existingRequest.SupportingDocuments = assistanceRequest.SupportingDocuments;
                    existingRequest.Status = assistanceRequest.Status;
                    existingRequest.ReviewNotes = assistanceRequest.ReviewNotes;
                    existingRequest.UpdatedAt = DateTime.Now;

                    // Allow branch change only by Admin
                    if (User.IsInRole("Admin") && assistanceRequest.BranchId.HasValue)
                    {
                        existingRequest.BranchId = assistanceRequest.BranchId;
                    }

                    // If status is being changed, set reviewer
                    if (previousStatus != assistanceRequest.Status)
                    {
                        existingRequest.ReviewedByUserId = _userManager.GetUserId(User);
                        existingRequest.ReviewedAt = DateTime.Now;
                        _context.ReviewAudits.Add(new ReviewAudit
                        {
                            AssistanceRequestId = existingRequest.Id,
                            Action = assistanceRequest.Status,
                            UserId = _userManager.GetUserId(User),
                            Timestamp = DateTime.UtcNow,
                            Notes = existingRequest.ReviewNotes
                        });
                    }

                    _context.Update(existingRequest);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تحديث طلب المساعدة بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssistanceRequestExists(assistanceRequest.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateFormOptionsAsync();
            ViewBag.Statuses = new List<string> { "معلق", "موافق عليه", "مرفوض", "تم التنفيذ" };
            return View(assistanceRequest);
        }

        // GET: AssistanceRequests/Delete/5
        [Authorize(Roles = "Admin,Branch Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var assistanceRequest = await _context.AssistanceRequests
                .Include(ar => ar.CreatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (assistanceRequest == null) return NotFound();

            return View(assistanceRequest);
        }

        // POST: AssistanceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Branch Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assistanceRequest = await _context.AssistanceRequests.FindAsync(id);
            if (assistanceRequest != null)
            {
                _context.AssistanceRequests.Remove(assistanceRequest);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف طلب المساعدة بنجاح";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: AssistanceRequests/Approve/5
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Approve(int id, string reviewNotes)
        {
            var assistanceRequest = await _context.AssistanceRequests.FindAsync(id);
            if (assistanceRequest == null) return RedirectToAction(nameof(Index));

            var currentUser = await _userManager.GetUserAsync(User);
            bool allowed = false;

            if (User.IsInRole("Admin") || User.IsInRole("Approver"))
            {
                allowed = true;
            }
            else if (User.IsInRole("Branch Manager"))
            {
                if (!string.IsNullOrEmpty(assistanceRequest.CreatedByUserId))
                {
                    var creator = await _userManager.FindByIdAsync(assistanceRequest.CreatedByUserId);
                    if (creator != null)
                    {
                        var creatorRoles = await _userManager.GetRolesAsync(creator);
                        if (creatorRoles.Contains("Staff") && creator.BranchId == currentUser?.BranchId)
                        {
                            allowed = true;
                        }
                    }
                }
            }

            if (!allowed)
            {
                TempData["ErrorMessage"] = "غير مسموح لك بمراجعة هذا الطلب.";
                return RedirectToAction(nameof(Index));
            }

            assistanceRequest.Status = "موافق عليه";
            assistanceRequest.ReviewedByUserId = _userManager.GetUserId(User);
            assistanceRequest.ReviewedAt = DateTime.Now;
            assistanceRequest.ReviewNotes = reviewNotes;
            assistanceRequest.UpdatedAt = DateTime.Now;
            // Ensure reference number exists
            if (string.IsNullOrWhiteSpace(assistanceRequest.ReferenceNumber))
            {
                assistanceRequest.ReferenceNumber = await GenerateUniqueReferenceAsync(assistanceRequest.BranchId);
            }
            _context.Update(assistanceRequest);
            _context.ReviewAudits.Add(new ReviewAudit
            {
                AssistanceRequestId = assistanceRequest.Id,
                Action = "موافق عليه",
                UserId = _userManager.GetUserId(User),
                Timestamp = DateTime.UtcNow,
                Notes = reviewNotes
            });
            // Also create an Assistance record to pass to cashier with the reference
            var beneficiary = await _context.Beneficiaries.FirstOrDefaultAsync(b => b.NationalId == assistanceRequest.BeneficiaryNationalId);
            if (beneficiary != null)
            {
                var assistance = new Assistance
                {
                    BeneficiaryId = beneficiary.Id,
                    Type = assistanceRequest.TypeOfAssistance ?? "",
                    Amount = assistanceRequest.Amount ?? 0m,
                    PaymentMethod = null,
                    Status = "ReadyForDisbursement",
                    Notes = $"مرجع الطلب: {assistanceRequest.ReferenceNumber}",
                    CreatedByUserId = _userManager.GetUserId(User) ?? string.Empty,
                    CreatedAt = DateTime.Now,
                    AssistanceRequestId = assistanceRequest.Id,
                    ReferenceNumber = assistanceRequest.ReferenceNumber
                };
                _context.Assistances.Add(assistance);

                // Create internal notifications for cashiers (prefer branch cashiers if branch assigned)
                var cashiers = new List<ApplicationUser>();
                if (assistanceRequest.BranchId.HasValue)
                {
                    var branchId = assistanceRequest.BranchId.Value;
                    var usersInBranch = await _context.Users.Where(u => u.BranchId == branchId).ToListAsync();
                    foreach (var u in usersInBranch)
                    {
                        var roles = await _userManager.GetRolesAsync(u);
                        if (roles.Contains("Cashier")) cashiers.Add(u);
                    }
                }

                if (!cashiers.Any())
                {
                    // fallback: all users in role Cashier
                    var usersInRole = await _userManager.GetUsersInRoleAsync("Cashier");
                    cashiers.AddRange(usersInRole);
                }

                var notifications = new List<Notification>();
                foreach (var cashier in cashiers.DistinctBy(u => u.Id))
                {
                    notifications.Add(new Notification
                    {
                        UserId = cashier.Id,
                        Message = $"يوجد مساعدة جاهزة للصرف. المرجع: {assistanceRequest.ReferenceNumber}",
                        Link = $"/Assistances/ReadyForDisbursement",
                        CreatedAt = DateTime.Now
                    });
                }

                if (notifications.Any()) _context.Notifications.AddRange(notifications);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم الموافقة على طلب المساعدة";
            return RedirectToAction(nameof(Index));
        }

        // POST: AssistanceRequests/Reject/5
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Reject(int id, string reviewNotes)
        {
            var assistanceRequest = await _context.AssistanceRequests.FindAsync(id);
            if (assistanceRequest == null) return RedirectToAction(nameof(Index));

            var currentUser = await _userManager.GetUserAsync(User);
            bool allowed = false;

            if (User.IsInRole("Admin") || User.IsInRole("Approver"))
            {
                allowed = true;
            }
            else if (User.IsInRole("Branch Manager"))
            {
                if (!string.IsNullOrEmpty(assistanceRequest.CreatedByUserId))
                {
                    var creator = await _userManager.FindByIdAsync(assistanceRequest.CreatedByUserId);
                    if (creator != null)
                    {
                        var creatorRoles = await _userManager.GetRolesAsync(creator);
                        if (creatorRoles.Contains("Staff") && creator.BranchId == currentUser?.BranchId)
                        {
                            allowed = true;
                        }
                    }
                }
            }

            if (!allowed)
            {
                TempData["ErrorMessage"] = "غير مسموح لك بمراجعة هذا الطلب.";
                return RedirectToAction(nameof(Index));
            }

            assistanceRequest.Status = "مرفوض";
            assistanceRequest.ReviewedByUserId = _userManager.GetUserId(User);
            assistanceRequest.ReviewedAt = DateTime.Now;
            assistanceRequest.ReviewNotes = reviewNotes;
            assistanceRequest.UpdatedAt = DateTime.Now;
            _context.Update(assistanceRequest);
            _context.ReviewAudits.Add(new ReviewAudit
            {
                AssistanceRequestId = assistanceRequest.Id,
                Action = "مرفوض",
                UserId = _userManager.GetUserId(User),
                Timestamp = DateTime.UtcNow,
                Notes = reviewNotes
            });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم رفض طلب المساعدة";
            return RedirectToAction(nameof(Index));
        }

        // GET: AssistanceRequests/AddAssistance/5
        [Authorize(Roles = "Admin,Branch Manager,Staff")]
        public async Task<IActionResult> AddAssistance(int? id)
        {
            if (id == null) return NotFound();

            var assistanceRequest = await _context.AssistanceRequests.FindAsync(id);
            if (assistanceRequest == null) return NotFound();

            // Only allow adding assistance if request is approved
            if (assistanceRequest.Status != "موافق عليه")
            {
                TempData["ErrorMessage"] = "لا يمكن صرف المساعدة إلا بعد الموافقة على الطلب.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(assistanceRequest);
        }

        // POST: AssistanceRequests/AddAssistance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Branch Manager,Staff")]
        public async Task<IActionResult> AddAssistance(int id, [Bind("Type,Amount,PaymentMethod,Notes")] Assistance assistance)
        {
            var assistanceRequest = await _context.AssistanceRequests.FindAsync(id);
            if (assistanceRequest == null) return NotFound();

            if (assistanceRequest.Status != "موافق عليه")
            {
                TempData["ErrorMessage"] = "لا يمكن صرف المساعدة إلا بعد الموافقة على الطلب.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Find the beneficiary by national id (created when request was submitted)
            var beneficiary = await _context.Beneficiaries.FirstOrDefaultAsync(b => b.NationalId == assistanceRequest.BeneficiaryNationalId);
            if (beneficiary == null)
            {
                TempData["ErrorMessage"] = "المستفيد غير موجود في السجل؛ لم يتم إنشاء المستفيد تلقائياً.";
                return RedirectToAction(nameof(Details), new { id });
            }

            assistance.BeneficiaryId = beneficiary.Id;
            assistance.AssistanceRequestId = id;
            assistance.CreatedByUserId = _userManager.GetUserId(User) ?? string.Empty;
            assistance.CreatedAt = DateTime.Now;
            assistance.Status = "Pending";

            _context.Assistances.Add(assistance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم إضافة المساعدة للطلب بنجاح";
            return RedirectToAction(nameof(Details), new { id });
        }

        private bool AssistanceRequestExists(int id)
        {
            return _context.AssistanceRequests.Any(e => e.Id == id);
        }

        private async Task<string> GenerateUniqueReferenceAsync(int? branchId = null)
        {
            // Format: REQ-YYYYMMDD-HHMMSS-XXXX
            string candidate;
            var rnd = new Random();
            do
            {
                if (branchId.HasValue)
                {
                    candidate = $"REQ-B{branchId.Value}-{DateTime.UtcNow:yyyy}-{DateTime.UtcNow:MMddHHmmss}-{rnd.Next(1000, 9999)}";
                }
                else
                {
                    candidate = $"REQ-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{rnd.Next(1000, 9999)}";
                }
            } while (await _context.AssistanceRequests.AnyAsync(ar => ar.ReferenceNumber == candidate));

            return candidate;
        }
    }
}
