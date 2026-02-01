using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MySQLMVC5Grid.Models
{

    [Table("transactions")]
    public class Transactions
    {
        [PrimaryKey, Identity] public int Id { get; set; }

        [Column, NotNull] public string TransactionId { get; set; }      // VARCHAR(50), UNIQUE
        [Column, NotNull] public int CustomerId { get; set; }
        [Column] public int? OrderId { get; set; }
        [Column] public string InvoiceNumber { get; set; }
        [Column] public string Description { get; set; }                  // VARCHAR(500)
        [Column, NotNull] public decimal Amount { get; set; }             // DECIMAL(15,2)
        [Column] public string CurrencyCode { get; set; }                 // VARCHAR(10)
        [Column] public string TransactionType { get; set; }              // VARCHAR(50)
        [Column] public string PaymentGateway { get; set; }               // VARCHAR(100)
        [Column, NotNull] public DateTime CreatedAt { get; set; }         // DEFAULT CURRENT_TIMESTAMP
        [Column] public DateTime? CompletedAt { get; set; }
        [Column] public string Status { get; set; }                       // VARCHAR(50)
    }

}