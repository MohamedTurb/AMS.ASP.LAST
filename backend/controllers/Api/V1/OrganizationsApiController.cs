using AssistanceManagementSystem.Contracts.Api;
using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Controllers.Api.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/organizations")]
    [Authorize(Policy = "ApiJwtPolicy", Roles = "Admin,Branch Manager")]
    [EnableRateLimiting("api")]
    [Produces("application/json")]
    public class OrganizationsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrganizationsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetAll()
        {
            var items = await _context.Organizations
                .AsNoTracking()
                .Select(x => new OrganizationDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type,
                    Address = x.Address,
                    Phone = x.Phone,
                    AccountNumber = x.AccountNumber
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrganizationDto>> GetById(int id)
        {
            var item = await _context.Organizations
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new OrganizationDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type,
                    Address = x.Address,
                    Phone = x.Phone,
                    AccountNumber = x.AccountNumber
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<OrganizationDto>> Create([FromBody] OrganizationDto model)
        {
            var entity = new Organization
            {
                Name = model.Name,
                Type = model.Type,
                Address = model.Address,
                Phone = model.Phone,
                AccountNumber = model.AccountNumber
            };

            _context.Organizations.Add(entity);
            await _context.SaveChangesAsync();

            model.Id = entity.Id;
            return CreatedAtAction(nameof(GetById), new { id = entity.Id, version = "1.0" }, model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrganizationDto model)
        {
            var entity = await _context.Organizations.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            entity.Type = model.Type;
            entity.Address = model.Address;
            entity.Phone = model.Phone;
            entity.AccountNumber = model.AccountNumber;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Organizations.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _context.Organizations.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
