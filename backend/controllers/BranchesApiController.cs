using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Controllers
{
    [Route("api/branches")]
    [ApiController]
    public class BranchesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BranchesApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/branches
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> List()
        {
            var branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();
            return Ok(branches);
        }

        // POST: api/branches
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Branch b)
        {
            _context.Branches.Add(b);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Dashboard), new { id = b.Id }, b);
        }

        // PUT: api/branches/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Branch b)
        {
            var existing = await _context.Branches.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = b.Name;
            existing.Address = b.Address;
            existing.Phone = b.Phone;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/branches/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var existing = await _context.Branches.FindAsync(id);
            if (existing == null) return NotFound();
            _context.Branches.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/branches/import
        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Import([FromBody] List<Branch> branches)
        {
            if (branches == null || !branches.Any()) return BadRequest();
            foreach (var b in branches)
            {
                _context.Branches.Add(b);
            }
            await _context.SaveChangesAsync();
            return Ok(new { Count = branches.Count });
        }

        // GET: api/branches/{id}/report?format=csv
        [HttpGet("{id}/report")]
        [Authorize(Roles = "Admin,Branch Manager")]
        public async Task<IActionResult> Report(int id, string format = "csv")
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Branch Manager") && currentUser?.BranchId != id)
            {
                return Forbid();
            }

            var q = _context.AssistanceRequests.Where(ar => ar.BranchId == id);
            var total = await q.CountAsync();
            var pending = await q.CountAsync(ar => ar.Status == "معلق");
            var approved = await q.CountAsync(ar => ar.Status == "موافق عليه");
            var rejected = await q.CountAsync(ar => ar.Status == "مرفوض");

            if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                var csv = "BranchId,BranchName,Total,Pending,Approved,Rejected\n" +
                          $"{id},\"{branch.Name}\",{total},{pending},{approved},{rejected}\n";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"branch-{id}-report.csv");
            }

            return Ok(new { id, branch.Name, total, pending, approved, rejected });
        }
        // GET: api/branches/{id}/dashboard
        [HttpGet("{id}/dashboard")]
        [Authorize(Roles = "Admin,Branch Manager")]
        public async Task<IActionResult> Dashboard(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Branch Manager") && currentUser?.BranchId != id)
            {
                return Forbid();
            }

            var q = _context.AssistanceRequests.AsQueryable().Where(ar => ar.BranchId == id);
            var total = await q.CountAsync();
            var pending = await q.CountAsync(ar => ar.Status == "معلق");
            var approved = await q.CountAsync(ar => ar.Status == "موافق عليه");
            var rejected = await q.CountAsync(ar => ar.Status == "مرفوض");
            var executed = await q.CountAsync(ar => ar.Status == "تم التنفيذ");
            var lastMonth = await q.CountAsync(ar => ar.CreatedAt >= DateTime.Today.AddDays(-30));

            return Ok(new
            {
                BranchId = id,
                BranchName = branch.Name,
                Total = total,
                Pending = pending,
                Approved = approved,
                Rejected = rejected,
                Executed = executed,
                LastMonth = lastMonth
            });
        }
    }
}
