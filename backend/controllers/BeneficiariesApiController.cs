using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Controllers
{
    [Route("api/beneficiaries")]
    [ApiController]
    public class BeneficiariesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BeneficiariesApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/beneficiaries/{id}/transfer
        [HttpPost("{id}/transfer")]
        [Authorize(Roles = "Admin,Branch Manager")]
        public async Task<IActionResult> Transfer(int id, [FromBody] TransferRequest req)
        {
            var beneficiary = await _context.Beneficiaries.FindAsync(id);
            if (beneficiary == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Branch Manager") && currentUser?.BranchId == null)
            {
                return Forbid();
            }

            var fromBranch = beneficiary.BranchId ?? 0;
            var toBranch = req.ToBranchId;
            if (toBranch == fromBranch) return BadRequest("ToBranchId must differ from current branch");

            // Branch Manager can only transfer out of their branch
            if (User.IsInRole("Branch Manager") && currentUser?.BranchId != fromBranch)
            {
                return Forbid();
            }

            beneficiary.BranchId = toBranch;
            _context.BeneficiaryTransfers.Add(new BeneficiaryTransfer
            {
                BeneficiaryId = beneficiary.Id,
                FromBranchId = fromBranch,
                ToBranchId = toBranch,
                PerformedByUserId = _userManager.GetUserId(User),
                Notes = req.Notes
            });

            await _context.SaveChangesAsync();
            return Ok(new { beneficiary.Id, beneficiary.BranchId });
        }

        public class TransferRequest
        {
            public int ToBranchId { get; set; }
            public string? Notes { get; set; }
        }
    }
}
