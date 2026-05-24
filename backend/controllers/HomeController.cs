using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            // If user is Beneficiary, show only their data
            if (userRoles.Contains("Beneficiary"))
            {
                var beneficiaryData = new BeneficiaryDashboardViewModel
                {
                    UserName = currentUser.FullName,
                    TotalRequests = await _context.AssistanceRequests
                        .Where(ar => ar.CreatedByUserId == currentUser.Id)
                        .CountAsync(),
                    PendingRequests = await _context.AssistanceRequests
                        .Where(ar => ar.CreatedByUserId == currentUser.Id && ar.Status == "معلق")
                        .CountAsync(),
                    ApprovedRequests = await _context.AssistanceRequests
                        .Where(ar => ar.CreatedByUserId == currentUser.Id && ar.Status == "موافق عليه")
                        .CountAsync(),
                    RejectedRequests = await _context.AssistanceRequests
                        .Where(ar => ar.CreatedByUserId == currentUser.Id && ar.Status == "مرفوض")
                        .CountAsync(),
                    ExecutedRequests = await _context.AssistanceRequests
                        .Where(ar => ar.CreatedByUserId == currentUser.Id && ar.Status == "تم التنفيذ")
                        .CountAsync(),
                    RecentRequests = await _context.AssistanceRequests
                        .Where(ar => ar.CreatedByUserId == currentUser.Id)
                        .OrderByDescending(ar => ar.CreatedAt)
                        .Take(5)
                        .ToListAsync()
                };

                return View("BeneficiaryDashboard", beneficiaryData);
            }

            // For other roles, show admin dashboard
            var dashboardData = new DashboardViewModel
            {
                TotalBeneficiaries = await _context.Beneficiaries.CountAsync(),
                TotalAssistanceRequests = await _context.AssistanceRequests.CountAsync(),
                TotalOrganizations = await _context.Organizations.CountAsync(),
                TotalProjects = await _context.Projects.CountAsync(),
                TotalAidCategories = await _context.AidCategories.CountAsync(),
                PendingRequests = await _context.AssistanceRequests.CountAsync(ar => ar.Status == "معلق"),
                ApprovedRequests = await _context.AssistanceRequests.CountAsync(ar => ar.Status == "موافق عليه"),
                RejectedRequests = await _context.AssistanceRequests.CountAsync(ar => ar.Status == "مرفوض"),
                ExecutedRequests = await _context.AssistanceRequests.CountAsync(ar => ar.Status == "تم التنفيذ"),
                RequestsThisMonth = await _context.AssistanceRequests.CountAsync(ar => ar.CreatedAt >= DateTime.Today.AddDays(-30)),
                RecentAssistanceRequests = await _context.AssistanceRequests
                    .OrderByDescending(ar => ar.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboardData);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }

    public class DashboardViewModel
    {
        public int TotalBeneficiaries { get; set; }
        public int TotalAssistanceRequests { get; set; }
        public int TotalOrganizations { get; set; }
        public int TotalProjects { get; set; }
        public int TotalAidCategories { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int ExecutedRequests { get; set; }
        public int RequestsThisMonth { get; set; }
        public List<AssistanceRequest> RecentAssistanceRequests { get; set; } = new();
    }

    public class BeneficiaryDashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int ExecutedRequests { get; set; }
        public List<AssistanceRequest> RecentRequests { get; set; } = new();
    }
}
