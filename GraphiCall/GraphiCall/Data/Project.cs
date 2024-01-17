using System.Text.Json.Serialization;

namespace GraphiCall.Data
{
    public class Project
    {
        public string ProjectId { get; set; } = null!;// NVARCHAR(450)

        // Podstawowe informacje o projekcie
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Nowe funkcjonalności
        public ProjectStatus Status { get; set; } // Status projektu
        public string ClientName { get; set; } = null!;    // Nazwa klienta

        // Relacja z ProjectTask: Wiele Task do jednego Project
        [JsonIgnore]
        public virtual ICollection<ProjectTask>? Tasks { get; set; }

        // Klucz obcy do ApplicationUser
        public string ApplicationUserId { get; set; } = null!;
        [JsonIgnore]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        // Relacja z Budget: Jeden Budget do jednego Project
        public string BudgetId { get; set; } = null!;
        [JsonIgnore]
        public virtual Budget? Budget { get; set; }
    }

    public enum ProjectStatus
    {
        NotStarted,
        InProgress,
        Completed,
        OnHold
    }

    public class ProjectTask
    {
        public string ProjectTaskId { get; set; } = null!; // NVARCHAR(450)
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime DueDate { get; set; }
        public TaskStatus Status { get; set; }

        // Klucz obcy do Project
        public string ProjectID { get; set; } = null!;
        [JsonIgnore]
        public virtual Project? Project { get; set; }
    }

    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Done
    }

    public class Budget
    {
        public string BudgetId { get; set; } = null!; // NVARCHAR(450)
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance => TotalIncome - TotalExpenses;
        public DateTime BudgetPeriod { get; set; }

        public string ProjectId { get; set; } = null!;
        [JsonIgnore]
        public virtual Project? Project { get; set; }

        // Relacja z Expense: Wiele Expense do jednego Budget
        public List<Expense>? Expenses { get; set; }

    }

    public class Expense
    {
        public string ExpenseId { get; set; } = null!; // NVARCHAR(450)
        public string Category { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime DateIncurred { get; set; }

        public string FK_BudgetId { get; set; } = null!;
        [JsonIgnore]
        public virtual Budget? Budget { get; set; }

    }

}
