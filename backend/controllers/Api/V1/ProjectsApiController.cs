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
    [Route("api/v{version:apiVersion}/projects")]
    [Authorize(Policy = "ApiJwtPolicy", Roles = "Admin,Branch Manager")]
    [EnableRateLimiting("api")]
    [Produces("application/json")]
    public class ProjectsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAll()
        {
            var items = await _context.Projects
                .AsNoTracking()
                .Select(x => new ProjectDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type,
                    Address = x.Address,
                    Phone = x.Phone
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProjectDto>> GetById(int id)
        {
            var item = await _context.Projects
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ProjectDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type,
                    Address = x.Address,
                    Phone = x.Phone
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDto>> Create([FromBody] ProjectDto model)
        {
            var entity = new Project
            {
                Name = model.Name,
                Type = model.Type,
                Address = model.Address,
                Phone = model.Phone
            };

            _context.Projects.Add(entity);
            await _context.SaveChangesAsync();

            model.Id = entity.Id;
            return CreatedAtAction(nameof(GetById), new { id = entity.Id, version = "1.0" }, model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectDto model)
        {
            var entity = await _context.Projects.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            entity.Type = model.Type;
            entity.Address = model.Address;
            entity.Phone = model.Phone;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Projects.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
