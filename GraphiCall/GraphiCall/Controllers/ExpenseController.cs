using GraphiCall.Client.DTO;
using GraphiCall.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GraphiCall.Controllers
{
    [ApiController]
    [Route("expenses")]
    public class ExpensesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/expenses/{userId}/{budgetId}/{expenseId}
        [HttpGet("getExpense/{userId}/budgets/{budgetId}/expense/{expenseId}")]
        public async Task<ActionResult<ExpenseDto>> GetExpense([FromRoute] string userId, [FromRoute] string budgetId, [FromRoute] string expenseId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(budgetId) || string.IsNullOrEmpty(expenseId))
            {
                return BadRequest("UserId, BudgetId, and ExpenseId are required.");
            }

            var expense = await _context.Expenses
                .Where(e => e.ExpenseId == expenseId && e.FK_BudgetId == budgetId && e.Budget.Project.ApplicationUserId == userId)
                .Select(e => new ExpenseDto
                {
                    ExpenseId = e.ExpenseId,
                    Category = e.Category,
                    Amount = e.Amount,
                    DateIncurred = e.DateIncurred,
                    FK_BudgetId = e.FK_BudgetId
                })
                .FirstOrDefaultAsync();

            return expense;
        }

        [HttpGet("getExpenses/{userId}/budgets/{budgetId}/allExpenses")]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetAllExpenses([FromRoute] string userId, [FromRoute] string budgetId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(budgetId))
            {
                return BadRequest("UserId and BudgetId are required.");
            }

            var expenses = await _context.Expenses
                .Where(e => e.FK_BudgetId == budgetId && e.Budget.Project.ApplicationUserId == userId)
                .Select(e => new ExpenseDto
                {
                    ExpenseId = e.ExpenseId,
                    Category = e.Category,
                    Amount = e.Amount,
                    DateIncurred = e.DateIncurred,
                    FK_BudgetId = e.FK_BudgetId
                })
                .ToListAsync();

            return expenses;
        }

        [HttpPost("addExpense/{userId}/budgets/{budgetId}")]
        public async Task<IActionResult> CreateExpense([FromRoute] string userId, [FromRoute] string budgetId, [FromBody] ExpenseDto expenseDto)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(budgetId))
            {
                return BadRequest("UserId and BudgetId are required.");
            }

            var expense = ConvertToExpense(expenseDto, budgetId);

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpPut("updateExpense/{userId}/budgets/{budgetId}/expense/{expenseId}")]
        public async Task<IActionResult> UpdateExpense([FromRoute] string userId, [FromRoute] string budgetId, [FromRoute] string expenseId, [FromBody] ExpenseDto expenseDto)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(budgetId) || string.IsNullOrEmpty(expenseId))
            {
                return BadRequest("UserId, BudgetId, and ExpenseId are required.");
            }

            if (expenseId != expenseDto.ExpenseId)
            {
                return BadRequest("Expense ID mismatch.");
            }

            var existingExpense = await _context.Expenses
                .Where(e => e.ExpenseId == expenseId && e.FK_BudgetId == budgetId && e.Budget.Project.ApplicationUserId == userId)
                .FirstOrDefaultAsync();

            if (existingExpense == null)
            {
                return NotFound("Expense not found.");
            }

            var updatedExpense = ConvertToExpense(expenseDto, budgetId);
            _context.Entry(existingExpense).CurrentValues.SetValues(updatedExpense);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseExists(expenseId))
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


        [HttpDelete("deleteExpense/{userId}/budgets/{budgetId}/expense/{expenseId}")]
        public async Task<IActionResult> DeleteExpense([FromRoute] string userId, [FromRoute] string budgetId, [FromRoute] string expenseId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(budgetId) || string.IsNullOrEmpty(expenseId))
            {
                return BadRequest("UserId, BudgetId, and ExpenseId are required.");
            }

            var expense = await _context.Expenses
                .Where(e => e.ExpenseId == expenseId && e.FK_BudgetId == budgetId && e.Budget.Project.ApplicationUserId == userId)
                .FirstOrDefaultAsync();

            if (expense == null)
            {
                return NotFound("Expense not found.");
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // HELPERS
        private bool ExpenseExists(string expenseId)
        {
            return _context.Expenses.Any(e => e.ExpenseId == expenseId);
        }

        private Expense ConvertToExpense(ExpenseDto expenseDto, string budgetId)
        {
            return new Expense
            {
                ExpenseId = expenseDto.ExpenseId,
                Category = expenseDto.Category,
                Amount = expenseDto.Amount,
                DateIncurred = expenseDto.DateIncurred,
                FK_BudgetId = budgetId
            };
        }
    }
}