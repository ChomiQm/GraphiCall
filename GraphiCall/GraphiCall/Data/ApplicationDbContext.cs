using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GraphiCall.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        public virtual DbSet<DataUser> DataUsers { get; set; }
        public virtual DbSet<Note> Notes { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectTask> ProjectTasks { get; set; }
        public virtual DbSet<Budget> Budgets { get; set; }
        public virtual DbSet<Expense> Expenses { get; set; }
        public virtual DbSet<Calendar> Calendars { get; set; }
        public virtual DbSet<CalendarEvent> CalendarEvents { get; set; }
        public virtual DbSet<Whiteboard> Whiteboards { get; set; }  
        public virtual DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(e => e.UserDateOfUpdate).HasDefaultValueSql("(getdate())").HasColumnType("datetime").HasColumnName("DateOfUpdate");
            });

            modelBuilder.Entity<DataUser>(entity =>
            {
                entity.ToTable("DataUsers");

                entity.HasKey(e => e.UserDataId);
                entity.HasOne(ud => ud.User).WithOne(u => u.DataUsers).HasForeignKey<DataUser>(ud => ud.UserId);
                entity.Property(e => e.UserFirstName).IsRequired().HasMaxLength(80).IsUnicode(true).HasColumnName("UserdataFirstName");
                entity.Property(e => e.UserSurname).IsRequired().HasMaxLength(80).IsUnicode(true).HasColumnName("UserdataSurname");
                entity.Property(e => e.UserCountry).IsRequired().HasMaxLength(80).IsUnicode(true).HasColumnName("UserdataCountry");
                entity.Property(e => e.UserTown).IsRequired().HasMaxLength(80).IsUnicode(true).HasColumnName("UserdataTown");
                entity.Property(e => e.UserStreet).IsRequired().HasMaxLength(80).IsUnicode(true).HasColumnName("UserdataStreet");
                entity.Property(e => e.UserHomeNumber).IsRequired().HasMaxLength(80).IsUnicode(true).HasColumnName("UserdataHomeNumber");
                entity.Property(e => e.UserFlatNumber).IsRequired().HasMaxLength(80).IsUnicode(true).HasColumnName("UserdataFlatNumber");
            });

            modelBuilder.Entity<Note>(entity =>
            {
                entity.ToTable("Notes");
                entity.HasKey(n => n.NoteId);
                entity.Property(n => n.NoteId).HasMaxLength(450);
                entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
                entity.Property(n => n.Content).IsRequired();
                entity.Property(n => n.CreatedAt).HasColumnType("datetime").IsRequired();
                entity.Property(n => n.UpdatedAt).HasColumnType("datetime");
                entity.Property(n => n.Priority).HasConversion<string>().IsRequired(); // Konwersja typu wyliczeniowego na string
                entity.Property(n => n.ApplicationUserId).IsRequired().HasMaxLength(450);

                entity.HasOne(n => n.ApplicationUser).WithMany(u => u.Notes).HasForeignKey(n => n.ApplicationUserId);
            });

            modelBuilder.Entity<Whiteboard>(entity =>
            {
                entity.ToTable("Whiteboards");
                entity.HasKey(e => e.WhiteboardId);
                entity.Property(e => e.WhiteboardId).HasMaxLength(450);
                entity.Property(e => e.Data).HasColumnType("nvarchar(max)"); // JSON data
                entity.Property(e => e.ApplicationUserId).IsRequired();
                entity.HasOne(w => w.ApplicationUser).WithOne(u => u.Whiteboard)
                      .HasForeignKey<Whiteboard>(w => w.ApplicationUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Calendar>(entity =>
            {
                entity.ToTable("Calendars");
                entity.HasKey(e => e.CalendarId);
                entity.Property(e => e.CalendarId).HasMaxLength(450);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500); // Opcjonalny
                entity.Property(e => e.ApplicationUserId).IsRequired();

                entity.HasOne(c => c.ApplicationUser).WithOne(u => u.Calendar).HasForeignKey<Calendar>(c => c.ApplicationUserId);
                entity.HasMany(c => c.Events).WithOne().HasForeignKey(ce => ce.FK_CalendarId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CalendarEvent>(entity =>
            {
                entity.ToTable("CalendarEvents");
                entity.HasKey(e => e.CalendarEventId);
                entity.Property(e => e.CalendarEventId).HasMaxLength(450); // Zmieniono na NVARCHAR(450)
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EventDate).HasColumnType("datetime").IsRequired();
                entity.Property(e => e.FromDate).HasColumnType("datetime");
                entity.Property(e => e.ToDate).HasColumnType("datetime");
                entity.Property(e => e.DateValue).HasMaxLength(50); // Dodano maksymaln¹ d³ugoœæ
                entity.Property(e => e.DayName).HasMaxLength(50); // Dodano maksymaln¹ d³ugoœæ
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.FK_CalendarId).IsRequired().HasMaxLength(450); // Zmieniono na NVARCHAR(450)
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
                entity.HasKey(p => p.ProjectId); // Klucz g³ówny
                entity.Property(p => p.ProjectId).HasMaxLength(450); // Ustawienie d³ugoœci dla ProjectId

                // Definicje dla pozosta³ych pól
                entity.Property(p => p.Name).IsRequired(); // Jeœli pole Name jest wymagane
                entity.Property(p => p.Description); // Konfiguracja dla Description
                entity.Property(p => p.StartDate); // Konfiguracja dla StartDate
                entity.Property(p => p.EndDate); // Konfiguracja dla EndDate
                entity.Property(p => p.Status); // Konfiguracja dla Status
                entity.Property(p => p.ClientName); // Konfiguracja dla ClientName

                // Relacja Project - ApplicationUser
                entity.HasOne(p => p.ApplicationUser)
                    .WithMany(u => u.Projects)
                    .HasForeignKey(p => p.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade); // Mo¿esz dostosowaæ zachowanie usuwania

                // Relacja Project - Budget
                entity.HasOne(p => p.Budget)
                    .WithOne(b => b.Project)
                    .HasForeignKey<Project>(p => p.BudgetId)
                    .OnDelete(DeleteBehavior.Cascade); // Mo¿esz dostosowaæ zachowanie usuwania

                // Relacja Project - ProjectTask
                entity.HasMany(p => p.Tasks)
                    .WithOne(t => t.Project)
                    .HasForeignKey(t => t.ProjectID)
                    .OnDelete(DeleteBehavior.Cascade); // Mo¿esz dostosowaæ zachowanie usuwania
            });

            modelBuilder.Entity<ProjectTask>(entity =>
            {
                entity.ToTable("ProjectTasks");
                entity.HasKey(t => t.ProjectTaskId); // Klucz g³ówny
                entity.Property(t => t.ProjectTaskId).HasMaxLength(450); // Ustawienie d³ugoœci dla ProjectTaskId

                // Definicje dla pozosta³ych pól
                entity.Property(t => t.Title).IsRequired(); // Jeœli pole Title jest wymagane
                entity.Property(t => t.Description); // Konfiguracja dla Description
                entity.Property(t => t.DueDate); // Konfiguracja dla DueDate
                entity.Property(t => t.Status); // Konfiguracja dla Status

                // Relacje
                // Relacja ProjectTask - Project
                entity.HasOne(t => t.Project)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(t => t.ProjectID)
                    .OnDelete(DeleteBehavior.Cascade); // Mo¿esz dostosowaæ zachowanie usuwania
            });

            modelBuilder.Entity<Budget>(entity =>
            {
                entity.ToTable("Budgets");
                entity.HasKey(b => b.BudgetId); // Klucz g³ówny
                entity.Property(b => b.BudgetId).HasMaxLength(450); // Ustawienie d³ugoœci dla BudgetId

                // Definicje dla pozosta³ych pól
                entity.Property(b => b.TotalIncome).HasColumnType("decimal(18, 2)"); ; // Konfiguracja dla TotalIncome
                entity.Property(b => b.TotalExpenses).HasColumnType("decimal(18, 2)"); ; // Konfiguracja dla TotalExpenses
                entity.Ignore(b => b.Balance); // Balance jest w³aœciwoœci¹ obliczan¹, wiêc jest ignorowana

                entity.Property(b => b.BudgetPeriod); // Konfiguracja dla BudgetPeriod

                // Relacje
                // Relacja Budget - Project
                entity.HasOne(b => b.Project)
                    .WithOne(p => p.Budget)
                    .HasForeignKey<Budget>(b => b.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade); // Mo¿esz dostosowaæ zachowanie usuwania

                // Relacja Budget - Expense
                entity.HasMany(b => b.Expenses)
                    .WithOne(e => e.Budget)
                    .HasForeignKey(e => e.FK_BudgetId)
                    .OnDelete(DeleteBehavior.Cascade); // Mo¿esz dostosowaæ zachowanie usuwania
            });

            modelBuilder.Entity<Expense>(entity =>
            {
                entity.ToTable("Expenses");
                entity.HasKey(e => e.ExpenseId); // Klucz g³ówny
                entity.Property(e => e.ExpenseId).HasMaxLength(450); // Ustawienie d³ugoœci dla ExpenseId

                // Definicje dla pozosta³ych pól
                entity.Property(e => e.Category).IsRequired(); // Jeœli pole Category jest wymagane
                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)"); ; // Konfiguracja dla Amount
                entity.Property(e => e.DateIncurred); // Konfiguracja dla DateIncurred

                // Relacja Expense - Budget
                entity.HasOne(e => e.Budget)
                    .WithMany(b => b.Expenses)
                    .HasForeignKey(e => e.FK_BudgetId)
                    .OnDelete(DeleteBehavior.Cascade); // Mo¿esz dostosowaæ zachowanie usuwania
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Messages");
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Id)
                      .IsRequired()
                      .HasMaxLength(450); // Ustawienie d³ugoœci klucza

                entity.Property(m => m.FromId)
                      .IsRequired()
                      .HasMaxLength(450); // J.w.

                entity.Property(m => m.ToId)
                      .IsRequired()
                      .HasMaxLength(450); // J.w.

                entity.Property(m => m.Content)
                      .HasMaxLength(1000); // Mo¿na dostosowaæ do w³asnych potrzeb

                entity.Property(m => m.SentOn)
                      .IsRequired();

                entity.HasOne(m => m.FromUser)
                      .WithMany() // Mo¿na dodaæ nawigacjê z powi¹zanej encji ApplicationUser, jeœli jest potrzebna
                      .HasForeignKey(m => m.FromId)
                      .OnDelete(DeleteBehavior.NoAction); // Mo¿na dostosowaæ strategiê usuwania

                entity.HasOne(m => m.ToUser)
                      .WithMany() // J.w.
                      .HasForeignKey(m => m.ToId)
                      .OnDelete(DeleteBehavior.NoAction); // J.w.
            });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

}