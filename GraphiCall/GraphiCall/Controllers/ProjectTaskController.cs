using GraphiCall.Client.DTO;
using GraphiCall.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GraphiCall.Controllers
{
    [ApiController]
    [Route("tasks")]
    public class ProjectTasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("getTasks/{userId}/projects/{projectId}/tasks")]
        public async Task<ActionResult<IEnumerable<ProjectTaskDto>>> GetProjectTasks([FromRoute] string userId, [FromRoute] string projectId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId))
            {
                return BadRequest("UserId and ProjectId are required.");
            }

            var projectTasks = await _context.ProjectTasks
                .Where(pt => pt.ProjectID == projectId && pt.Project.ApplicationUserId == userId)
                .Select(pt => new ProjectTaskDto
                {
                    ProjectTaskId = pt.ProjectTaskId,
                    Title = pt.Title,
                    Description = pt.Description,
                    DueDate = pt.DueDate,
                    Status = (Client.DTO.TaskStatus)pt.Status,
                    ProjectID = pt.ProjectID
                })
                .ToListAsync();

            return projectTasks;
        }

        [HttpPost("addTask/{userId}/projects/{projectId}")]
        public async Task<IActionResult> CreateProjectTask([FromRoute] string userId, [FromRoute] string projectId, [FromBody] ProjectTaskDto projectTaskDto)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId))
            {
                return BadRequest("UserId and ProjectId are required.");
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.ApplicationUserId == userId);

            if (project == null)
            {
                return NotFound("Project not found or access denied.");
            }

            var projectTask = ConvertToProjectTask(projectTaskDto);
            projectTask.ProjectID = projectId; // Ensure the project ID is set correctly

            _context.ProjectTasks.Add(projectTask);
            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpPut("updateTask/{userId}/projects/{projectId}/task/{projectTaskId}")]
        public async Task<IActionResult> UpdateProjectTask([FromRoute] string userId, [FromRoute] string projectId, [FromRoute] string projectTaskId, [FromBody] ProjectTaskDto projectTaskDto)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(projectTaskId))
            {
                return BadRequest("UserId, ProjectId, and ProjectTaskId are required.");
            }

            if (projectTaskId != projectTaskDto.ProjectTaskId)
            {
                return BadRequest("ProjectTask ID mismatch.");
            }

            var existingProjectTask = await _context.ProjectTasks
                .Where(pt => pt.ProjectTaskId == projectTaskId && pt.ProjectID == projectId && pt.Project.ApplicationUserId == userId)
                .FirstOrDefaultAsync();

            if (existingProjectTask == null)
            {
                return NotFound("Project task not found.");
            }

            var updatedProjectTask = ConvertToProjectTask(projectTaskDto);
            _context.Entry(existingProjectTask).CurrentValues.SetValues(updatedProjectTask);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectTaskExists(projectTaskId))
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

        [HttpDelete("deleteTask/{userId}/projects/{projectId}/task/{projectTaskId}")]
        public async Task<IActionResult> DeleteProjectTask([FromRoute] string userId, [FromRoute] string projectId, [FromRoute] string projectTaskId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(projectTaskId))
            {
                return BadRequest("UserId, ProjectId, and ProjectTaskId are required.");
            }

            var projectTask = await _context.ProjectTasks
                .Where(pt => pt.ProjectTaskId == projectTaskId && pt.ProjectID == projectId && pt.Project.ApplicationUserId == userId)
                .FirstOrDefaultAsync();

            if (projectTask == null)
            {
                return NotFound("Project task not found.");
            }

            _context.ProjectTasks.Remove(projectTask);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // HELPERS
        private bool ProjectTaskExists(string projectTaskId)
        {
            return _context.ProjectTasks.Any(e => e.ProjectTaskId == projectTaskId);
        }

        private ProjectTask ConvertToProjectTask(ProjectTaskDto projectTaskDto)
        {
            return new ProjectTask
            {
                ProjectTaskId = projectTaskDto.ProjectTaskId,
                Title = projectTaskDto.Title,
                Description = projectTaskDto.Description,
                DueDate = projectTaskDto.DueDate,
                Status = (Data.TaskStatus)projectTaskDto.Status,
                ProjectID = projectTaskDto.ProjectID
            };
        }

    }
}
