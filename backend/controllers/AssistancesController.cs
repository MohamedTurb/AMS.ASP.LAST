using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Controllers
{
    [Authorize(Roles = "Cashier,Admin,Branch Manager")]
    public class AssistancesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssistancesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Assistances/ReadyForDisbursement
        public async Task<IActionResult> ReadyForDisbursement(string search)
        {
            var query = _context.Assistances
                .Include(a => a.Beneficiary)
                .Include(a => a.AssistanceRequest)
                .AsQueryable();

            query = query.Where(a => a.Status == "ReadyForDisbursement");

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => (a.ReferenceNumber ?? "").Contains(search) || (a.AssistanceRequest != null && (a.AssistanceRequest.ReferenceNumber ?? "").Contains(search)));
            }

            var list = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
            return View(list);
        }
    }
}
