using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft_SQL_Server_MVC.Models
{
    public class Orders
    {
        public int OrderID { get; set; }
        public string CustomerID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShipCity { get; set; }
        public decimal Freight { get; set; }

        // Sample Data
        public static List<Orders> GetAllRecords()
        {
            return new List<Orders>()
            {
                new Orders() { OrderID = 10248, CustomerID = "VINET", EmployeeID = 5, OrderDate = DateTime.Now.AddDays(-10), ShipCity = "Reims", Freight = 32.38M },
                new Orders() { OrderID = 10249, CustomerID = "TOMSP", EmployeeID = 6, OrderDate = DateTime.Now.AddDays(-8), ShipCity = "Münster", Freight = 11.61M },
                new Orders() { OrderID = 10250, CustomerID = "HANAR", EmployeeID = 4, OrderDate = DateTime.Now.AddDays(-6), ShipCity = "Rio de Janeiro", Freight = 65.83M }
            };
        }
    }
}