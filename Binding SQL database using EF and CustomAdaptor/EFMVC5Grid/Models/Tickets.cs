using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFMVC5Grid.Models
{
    /// <summary>
    /// Represents a ticket record mapped to the 'Tickets' table in the database.
    /// This model defines the structure of ticket-related data used throughout the application.
    /// </summary>

    /// <summary>
    /// Represents a Network Support ticket.
    /// </summary>
    [Table("Tickets", Schema = "dbo")]
    public class Tickets
    {
        /// <summary>
        /// Surrogate primary key. Identity(1,1).
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketId { get; set; }

        /// <summary>
        /// Public-facing ticket identifier (e.g., NET-1001). UNIQUE, NOT NULL, VARCHAR(50).
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Index("IX_Tickets_PublicTicketId", IsUnique = true)]   // EF6 [Index] attribute (System.ComponentModel.DataAnnotations.Schema, EF 6.1+)
        public string PublicTicketId { get; set; }

        /// <summary>
        /// Title/subject. VARCHAR(200) NULL.
        /// </summary>
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Detailed description. TEXT NULL (SQL Server TEXT is deprecated; consider NVARCHAR(MAX) instead).
        /// </summary>
        [Column(TypeName = "text")]
        public string Description { get; set; }

        /// <summary>
        /// Category (e.g., Network, Hardware, Software). VARCHAR(100) NULL.
        /// </summary>
        [MaxLength(100)]
        public string Category { get; set; }

        /// <summary>
        /// Department responsible. VARCHAR(100) NULL.
        /// </summary>
        [MaxLength(100)]
        public string Department { get; set; }

        /// <summary>
        /// Assigned agent. VARCHAR(100) NULL.
        /// </summary>
        [MaxLength(100)]
        public string Assignee { get; set; }

        /// <summary>
        /// Creator name. VARCHAR(100) NULL.
        /// </summary>
        [MaxLength(100)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Status (NOT NULL, DEFAULT 'Open'). VARCHAR(50).
        /// NOTE: EF6 cannot declare defaults via annotations; keep Required here and let DB default apply when unset.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        /// <summary>
        /// Priority (NOT NULL, DEFAULT 'Medium'). VARCHAR(50).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Priority { get; set; }

        /// <summary>
        /// Response due date. DATETIME2 NULL.
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? ResponseDue { get; set; }

        /// <summary>
        /// Resolution due date. DATETIME2 NULL.
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Created timestamp (NOT NULL, DEFAULT GETDATE()). DATETIME2.
        /// Marked as DatabaseGenerated so EF lets the DB fill it and will re-query on SaveChanges.
        /// </summary>
        [Required]
        [Column(TypeName = "datetime2")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last updated timestamp (NOT NULL, DEFAULT GETDATE()). DATETIME2.
        /// Mark as DatabaseGenerated so DB can set a default on insert; for updates, set in code or trigger.
        /// </summary>
        [Required]
        [Column(TypeName = "datetime2")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }
    }
}