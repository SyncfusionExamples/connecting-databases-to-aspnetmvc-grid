using LinqToDB.Mapping;

namespace MSSQLMVC5Grid.Models
{
    // Use fully qualified attribute names to resolve ambiguity between LinqToDB.Mapping.TableAttribute and System.ComponentModel.DataAnnotations.Schema.TableAttribute

    // For LinqToDB mapping:
    [Table("Tickets", Schema = "dbo")]
    public sealed class Tickets
    {
        [PrimaryKey, Identity] public int TicketId { get; set; }

        [Column, NotNull] public string PublicTicketId { get; set; }
        [Column] public string Title { get; set; }
        [Column] public string Description { get; set; }
        [Column] public string Category { get; set; }
        [Column] public string Department { get; set; }
        [Column] public string Assignee { get; set; }
        [Column] public string CreatedBy { get; set; }
        [Column, NotNull] public string Status { get; set; }
        [Column, NotNull] public string Priority { get; set; }
        [Column] public System.DateTime? ResponseDue { get; set; }
        [Column] public System.DateTime? DueDate { get; set; }

        // Remove non-existent SkipOnInsert and SkipOnUpdate attributes.
        [Column, NotNull] public System.DateTime CreatedAt { get; set; }
        [Column, NotNull] public System.DateTime UpdatedAt { get; set; }
    }
}