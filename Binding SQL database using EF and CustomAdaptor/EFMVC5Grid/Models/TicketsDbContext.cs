using System.Data.Entity;


namespace EFMVC5Grid.Models
{
    /// <summary>
    /// DbContext for Tickets entity
    /// Manages database connections and entity configurations for the Network Support Ticket System
    /// </summary>
    public class TicketsDbContext : DbContext
    {
        public TicketsDbContext()
            : base("name=TicketsDbContext")
        {
            // Enable SQL logging to debug output
            this.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }

        /// <summary>
        /// DbSet for Ticket entities
        /// </summary>
        public DbSet<Tickets> Tickets => Set<Tickets>();

        /// <summary>
        /// Configures the entity mappings and constraints
        /// </summary>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Tickets entity
            var entity = modelBuilder.Entity<Tickets>();

            // Primary Key
            entity.HasKey(e => e.TicketId);

            // Auto-increment for Primary Key
            entity.Property(e => e.TicketId)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);

            // Column configurations
            entity.Property(e => e.PublicTicketId)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsOptional();

            entity.Property(e => e.Description)
                .HasMaxLength(int.MaxValue) // For MAX type
                .IsOptional();

            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsOptional();

            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .IsOptional();

            entity.Property(e => e.Assignee)
                .HasMaxLength(100)
                .IsOptional();

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsOptional();

            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsOptional();

            entity.Property(e => e.Priority)
                .HasMaxLength(50)
                .IsOptional();

            // DateTime columns
            entity.Property(e => e.ResponseDue)
                .HasColumnType("datetime2")
                .IsOptional();

            entity.Property(e => e.DueDate)
                .HasColumnType("datetime2")
                .IsOptional();

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime2")
                .IsOptional();

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime2")
                .IsOptional();

            // Table name and schema
            entity.ToTable("Tickets", "dbo");
        }
    }
}