using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Syncfusion.EJ2.Base;

namespace Grid_EntityFramework.Controllers
{
    public class GridController : Controller
    {
        /// <summary>
        /// Processes the DataManager request to perform searching, filtering, sorting, and paging operations.
        /// </summary>
        /// <param name="DataManagerRequest">Contains the details of the data operation requested.</param>
        /// <returns>Returns a JSON object with the filtered, sorted, and paginated data along with the total count.</returns>
        public JsonResult UrlDataSource(DataManagerRequest DataManagerRequest)
        {
            using (var Context = new OrderDbContext())
            {
                IQueryable<Orders> DataSource = Context.Orders.AsQueryable();
                QueryableOperation queryableOperation = new QueryableOperation();

                if (DataManagerRequest.Search != null && DataManagerRequest.Search.Count > 0)
                {
                    DataSource = queryableOperation.PerformSearching(DataSource, DataManagerRequest.Search);
                }

                if (DataManagerRequest.Where != null && DataManagerRequest.Where.Count > 0)
                {
                    foreach (WhereFilter condition in DataManagerRequest.Where)
                    {
                        foreach (WhereFilter predicate in condition.predicates)
                        {
                            DataSource = queryableOperation.PerformFiltering(DataSource, DataManagerRequest.Where, predicate.Operator);
                        }
                    }
                }

                if (DataManagerRequest.Sorted != null && DataManagerRequest.Sorted.Count > 0)
                {
                    DataSource = queryableOperation.PerformSorting(DataSource, DataManagerRequest.Sorted);
                }
                else
                {
                    // Default sorting by OrderID to prevent Skip error
                    DataSource = DataSource.OrderBy(o => o.OrderID);
                }
                int totalRecordsCount = DataSource.Count();

                if (DataManagerRequest.Skip != 0)
                {
                    DataSource = queryableOperation.PerformSkip(DataSource, DataManagerRequest.Skip);
                }

                if (DataManagerRequest.Take != 0)
                {
                    DataSource = queryableOperation.PerformTake(DataSource, DataManagerRequest.Take);
                }

                return Json(new { result = DataSource.ToList(), count = totalRecordsCount }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// Inserts a new data item into the data collection.
        /// </summary>
        public JsonResult Insert(Orders value)
        {
            if (value == null)
            {
                return Json(new { success = false, message = "Inserted entity cannot be null." });
            }

            using (var Context = new OrderDbContext())
            {
                Context.Orders.Add(value);
                Context.SaveChanges();
            }

            return Json(new { success = true, message = "Insert successful", data = value });
        }

        /// <summary>
        /// Updates an existing data item in the data collection.
        /// </summary>
        public void Update(Orders value)
        {
            using (var Context = new OrderDbContext())
            {
                Orders existingOrder = Context.Orders.Find(value.OrderID);
                if (existingOrder != null)
                {
                    Context.Entry(existingOrder).CurrentValues.SetValues(value);
                    Context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Removes a specific data item from the data collection.
        /// </summary>
        public void Remove(CRUDModel<Orders> value)
        {
            int OrderId = Convert.ToInt32(value.key.ToString());
            using (var Context = new OrderDbContext())
            {
                Orders Order = Context.Orders.Find(OrderId);
                if (Order != null)
                {
                    Context.Orders.Remove(Order);
                    Context.SaveChanges();
                }
            }
        }

        public JsonResult BatchUpdate(CRUDModel<Orders> value)
        {
            using (var Context = new OrderDbContext())
            {
                // Handle changed records
                if (value.changed != null && value.changed.Count > 0)
                {
                    foreach (Orders Record in value.changed)
                    {
                        Context.Orders.Attach(Record);
                        Context.Entry(Record).State = EntityState.Modified;
                    }
                }

                // Handle added records
                if (value.added != null && value.added.Count > 0)
                {
                    foreach (Orders order in value.added)
                    {
                        order.OrderID = default;
                        Context.Orders.Add(order);
                    }
                }

                // Handle deleted records
                if (value.deleted != null && value.deleted.Count > 0)
                {
                    foreach (Orders Record in value.deleted)
                    {
                        var existingOrder = Context.Orders.Find(Record.OrderID);
                        if (existingOrder != null)
                        {
                            Context.Orders.Remove(existingOrder);
                        }
                    }
                }

                // Save all changes to the database
                Context.SaveChanges();
            }
            return Json(value, JsonRequestBehavior.AllowGet);
        }

    }

    /// <summary>
    /// Entity Framework DbContext for Orders.
    /// </summary>
    public class OrderDbContext : DbContext
    {
        public OrderDbContext() : base("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\NithyaSivaprakasam\\Downloads\\NORTHWND.MDF;Integrated Security=True;Connect Timeout=30;") { }
        public DbSet<Orders> Orders { get; set; }
    }

    public class CRUDModel<T> where T : class
    {
        public string action { get; set; }
        public string keyColumn { get; set; }
        public object key { get; set; }
        public T value { get; set; }
        public List<T> added { get; set; }
        public List<T> changed { get; set; }
        public List<T> deleted { get; set; }
        public IDictionary<string, object> @params { get; set; }
    }

    public class Orders
    {
        [Key]
        public int OrderID { get; set; }
        public string CustomerID { get; set; }
        public int EmployeeID { get; set; }
        public decimal Freight { get; set; }
        public string ShipCity { get; set; }
        public string ShipName { get; set; }
        public string ShipCountry { get; set; }
        public string ShipAddress { get; set; }
    }
}
