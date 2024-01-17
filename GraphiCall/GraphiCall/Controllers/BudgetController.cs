using GraphiCall.Client.DTO;
using GraphiCall.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraphiCall.Controllers
{
    [ApiController]
    [Route("budgets")]
    public class BudgetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BudgetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/budget/getBudget/{userId}/{projectId}/{budgetId}
        [HttpGet("getBudget/{userId}/project/{projectId}/budget/{budgetId}")]
        public async Task<ActionResult<Budget>> GetBudget([FromRoute] string userId, [FromRoute] string projectId, [FromRoute] string budgetId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(budgetId))
            {
                return BadRequest("UserId, ProjectId, and BudgetId are required.");
            }

            var budget = await _context.Budgets
                .Where(b => b.BudgetId == budgetId && b.ProjectId == projectId && b.Project.ApplicationUserId == userId)
                .Include(b => b.Expenses)
                .FirstOrDefaultAsync();

            if (budget == null)
            {
                return NotFound("Budget not found.");
            }

            return budget;
        }

        [HttpPost("addBudget/{userId}/project/{projectId}")]
        public async Task<ActionResult<BudgetDto>> CreateBudget([FromRoute] string userId, [FromRoute] string projectId, [FromBody] BudgetDto budgetDto)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(projectId))
            {
                return BadRequest("Project ID is required.");
            }

            if (budgetDto == null)
            {
                return BadRequest("Budget data is required.");
            }

            var budget = ConvertToBudget(budgetDto);
            budget.ProjectId = projectId; // Upewnij się, że ID projektu jest przypisane do budżetu

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            return Ok(budgetDto);
        }

        [HttpPut("updateBudget/{userId}/project/{projectId}/budget/{budgetId}")]
        public async Task<IActionResult> UpdateBudget([FromRoute] string userId, [FromRoute] string projectId, [FromRoute] string budgetId, [FromBody] BudgetDto budgetDto)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(budgetId))
            {
                return BadRequest("UserId, ProjectId, and BudgetId are required.");
            }

            if (budgetId != budgetDto.BudgetId)
            {
                return BadRequest("Budget ID mismatch.");
            }

            var existingBudget = await _context.Budgets
                .Where(b => b.BudgetId == budgetId && b.ProjectId == projectId && b.Project.ApplicationUserId == userId)
                .FirstOrDefaultAsync();

            if (existingBudget == null)
            {
                return NotFound("Budget not found.");
            }

            var updatedBudget = ConvertToBudget(budgetDto);
            _context.Entry(existingBudget).CurrentValues.SetValues(updatedBudget);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BudgetExists(budgetId))
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

        [HttpDelete("deleteBudget/{userId}/project/{projectId}/budget/{budgetId}")]
        public async Task<IActionResult> DeleteBudget([FromRoute] string userId, [FromRoute] string projectId, [FromRoute] string budgetId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(budgetId))
            {
                return BadRequest("UserId, ProjectId, and BudgetId are required.");
            }

            var budget = await _context.Budgets
                .Where(b => b.BudgetId == budgetId && b.ProjectId == projectId && b.Project.ApplicationUserId == userId)
                .FirstOrDefaultAsync();

            if (budget == null)
            {
                return NotFound("Budget not found.");
            }

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //HELPERS
        private bool BudgetExists(string budgetId)
        {
            return _context.Budgets.Any(e => e.BudgetId == budgetId);
        }

        private Budget ConvertToBudget(BudgetDto budgetDto)
        {
            var budget = new Budget
            {
                BudgetId = budgetDto.BudgetId,
                TotalIncome = budgetDto.TotalIncome,
                TotalExpenses = budgetDto.TotalExpenses,
                BudgetPeriod = budgetDto.BudgetPeriod,
                ProjectId = budgetDto.ProjectId
            };
            return budget;
        }
    }
}
