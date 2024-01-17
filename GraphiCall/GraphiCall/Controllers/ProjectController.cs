using GraphiCall.Client.DTO;
using GraphiCall.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraphiCall.Controllers
{
    [Route("projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("getAllProjects/{userId}/with-everything")]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects([FromRoute] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var projects = await _context.Projects
                .Where(p => p.ApplicationUserId == userId)
                .Include(p => p.Tasks)
                .Include(p => p.Budget).ThenInclude(b => b.Expenses)
                .ToListAsync();

            if (!projects.Any())
            {
                return NotFound("No projects found for this user.");
            }

            return Ok(projects);
        }

        [HttpGet("getProject/{userId}/{projectId}/with-everything")]
        public async Task<ActionResult<Project>> GetProject([FromRoute] string userId, [FromRoute] string projectId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId))
            {
                return BadRequest("UserId and ProjectId are required.");
            }

            var project = await _context.Projects
                .Where(p => p.ProjectId == projectId && p.ApplicationUserId == userId)
                .Include(p => p.Tasks)
                .Include(p => p.Budget).ThenInclude(b => b.Expenses)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound("Project not found for the given user.");
            }

            return Ok(project);
        }

        [HttpPost("addProject/{userId}")]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromRoute] string userId, [FromBody] ProjectDto projectDto)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (projectDto == null)
            {
                return BadRequest("Project data is required.");
            }

            var project = ConvertToProject(projectDto);

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return Ok(projectDto);
        }

        [HttpPut("updateProject/{userId}/{projectId}/Params")]
        public async Task<IActionResult> UpdateProject([FromRoute] string userId, [FromRoute] string projectId, [FromBody] ProjectDto projectDto)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (projectId != projectDto.ProjectId)
            {
                return BadRequest("Project ID mismatch.");
            }

            var project = await _context.Projects
                .Where(p => p.ProjectId == projectId && p.ApplicationUserId == userId)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound("Project not found.");
            }

            // Aktualizacja pól projektu
            project.Name = projectDto.Name;
            project.Description = projectDto.Description;
            project.StartDate = projectDto.StartDate;
            project.EndDate = projectDto.EndDate;
            project.Status = (Data.ProjectStatus)projectDto.Status;
            project.ClientName = projectDto.ClientName;

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(projectId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("deleteProject/{userId}/{projectId}")]
        public async Task<IActionResult> DeleteProject([FromRoute] string userId, [FromRoute] string projectId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId))
            {
                return BadRequest("UserId and ProjectId are required.");
            }

            var project = await _context.Projects
                .Where(p => p.ProjectId == projectId && p.ApplicationUserId == userId)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound("Project not found.");
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // HELPERS

        [HttpGet("hasProjects/{userId}")]
        public async Task<ActionResult<bool>> UserHasProjects([FromRoute] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var hasProjects = await _context.Projects.AnyAsync(p => p.ApplicationUserId == userId);
            return Ok(hasProjects);
        }

        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }

        private Project ConvertToProject(ProjectDto projectDto)
        {
            var project = new Project
            {
                ProjectId = projectDto.ProjectId,
                Name = projectDto.Name,
                Description = projectDto.Description,
                StartDate = projectDto.StartDate,
                EndDate = projectDto.EndDate,
                Status = (Data.ProjectStatus)projectDto.Status,
                ClientName = projectDto.ClientName,
                ApplicationUserId = projectDto.ApplicationUserId,
                BudgetId = projectDto.BudgetId
            };
            return project;
        }

    }

}
