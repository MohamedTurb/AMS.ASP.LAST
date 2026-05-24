using AssistanceManagementSystem.Contracts.Api;
using AssistanceManagementSystem.Data;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Controllers.Api.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/assistance-requests")]
    [Authorize(Policy = "ApiJwtPolicy")]
    [EnableRateLimiting("api")]
    [Produces("application/json")]
    public class AssistanceRequestsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssistanceRequestsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssistanceRequestSummaryDto>>> GetAll([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

            var query = _context.AssistanceRequests.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AssistanceRequestSummaryDto
                {
                    Id = x.Id,
                    RequesterName = x.RequesterName,
                    BeneficiaryName = x.BeneficiaryName,
                    TypeOfAssistance = x.TypeOfAssistance,
                    AidCategoryName = x.AidCategory != null ? x.AidCategory.NameAr : null,
                    AidCategoryDetails = x.AidCategoryDetails,
                    Amount = x.Amount,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AssistanceRequestSummaryDto>> GetById(int id)
        {
            var item = await _context.AssistanceRequests
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new AssistanceRequestSummaryDto
                {
                    Id = x.Id,
                    RequesterName = x.RequesterName,
                    BeneficiaryName = x.BeneficiaryName,
                    TypeOfAssistance = x.TypeOfAssistance,
                    AidCategoryName = x.AidCategory != null ? x.AidCategory.NameAr : null,
                    AidCategoryDetails = x.AidCategoryDetails,
                    Amount = x.Amount,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }
    }
}
