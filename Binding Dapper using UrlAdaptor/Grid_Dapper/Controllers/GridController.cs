using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Syncfusion.EJ2.Base;
using Syncfusion.EJ2.Linq;
using Dapper;

namespace Grid_Dapper.Controllers
{
    public class GridController : Controller
    {
        /// <summary>
        /// Connection string for the database.
        /// </summary>
        private readonly string ConnectionString = @"<Enter a valid connection string>";

        /// <summary>
        ///  Retrieves the order data from the database.
        /// </summary>
        /// <returns>Returns a JSON result containing the list of orders and total count.</returns>
        public JsonResult UrlDataSource(DataManagerRequest DataManagerRequest)
        {
            // Retrieve data from the data source (e.g., database).
            IQueryable<Orders> dataSource = GetOrderData().AsQueryable();

            // Initialize QueryableOperation instance.
            QueryableOperation queryableOperation = new QueryableOperation();

            // Handling searching operation.
            if (DataManagerRequest.Search?.Count > 0)
            {
                dataSource = queryableOperation.PerformSearching(dataSource, DataManagerRequest.Search);
                //Add custom logic here if needed and remove above method.
            }

            // Handling filtering operation.
            if (DataManagerRequest.Where?.Count > 0)
            {
                foreach (WhereFilter condition in DataManagerRequest.Where)
                {
                    foreach (WhereFilter predicate in condition.predicates)
                    {
                        dataSource = queryableOperation.PerformFiltering(dataSource, DataManagerRequest.Where, predicate.Operator);
                        //Add custom logic here if needed and remove above method.
                    }
                }
            }

            // Handling sorting operation.
            if (DataManagerRequest.Sorted?.Count > 0)
            {
                dataSource = queryableOperation.PerformSorting(dataSource, DataManagerRequest.Sorted);
                //Add custom logic here if needed and remove above method.
            }

            // Get the total count of records.
            int totalRecordsCount = dataSource.Count();

            // Handling paging operation.
            if (DataManagerRequest.Skip > 0)
            {
                dataSource = queryableOperation.PerformSkip(dataSource, DataManagerRequest.Skip);
            }
            if (DataManagerRequest.Take > 0)
            {
                dataSource = queryableOperation.PerformTake(dataSource, DataManagerRequest.Take);
            }

            // Return data based on the request.
            return Json(new { result = dataSource, count = totalRecordsCount }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Retrieves order data from the database.
        /// </summary>
        /// <returns>Returns a list of orders fetched from the database.</returns>
        private List<Orders> GetOrderData()
        {
            string queryString = "SELECT * FROM dbo.Orders ORDER BY OrderID";

            //Create SQL connection.
            using (IDbConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                // Dapper automatically handles mapping to your orders class.
                return connection.Query<Orders>(queryString).ToList();
            }
        }

        /// <summary>
        /// Inserts a new data item into the data collection.
        /// </summary>
        /// <param name="newRecord">It contains the new record detail which is need to be inserted.</param>
        /// <returns>Returns void.</returns>
        public void Insert(CRUDModel<Orders> newRecord)
        {
            //Create query to insert the specific into the database by accessing its properties.
            string queryStr = "INSERT INTO Orders(CustomerID, Freight, ShipCity, EmployeeID) VALUES(@CustomerID, @Freight, @ShipCity, @EmployeeID)";

            //Create SQL connection.
            using (IDbConnection Connection = new SqlConnection(ConnectionString))
            {
                Connection.Open();

                //Execute this code to reflect the changes into the database.
                Connection.Execute(queryStr, newRecord.value);
            }

            //Add custom logic here if needed and remove above method.
        }

        /// <summary>
        /// Update a existing data item from the data collection.
        /// </summary>
        /// <param name="updateOrder">It contains the updated record detail which is need to be updated.</param>
        /// <returns>Returns void.</returns>
        public void Update(CRUDModel<Orders> updateOrder)
        {
            //Create query to update the changes into the database by accessing its properties.
            string queryStr = "UPDATE Orders SET CustomerID = @CustomerID, Freight = @Freight, ShipCity = @ShipCity, EmployeeID = @EmployeeID WHERE OrderID = @OrderID";

            //Create SQL connection.
            using (IDbConnection Connection = new SqlConnection(ConnectionString))
            {
                Connection.Open();

                //Execute this code to reflect the changes into the database.
                Connection.Execute(queryStr, updateOrder.value);
            }

            //Add custom logic here if needed and remove above method.
        }

        /// <summary>
        /// Remove a specific data item from the data collection.
        /// </summary>
        /// <param name="value">It contains the specific record detail which is need to be removed.</param>
        /// <return>Returns void.</return>
        public void Remove(CRUDModel<Orders> value)
        {
            //Create query to remove the specific from database by passing the primary key column value.
            string queryStr = "DELETE FROM Orders WHERE OrderID = @OrderID";

            //Create SQL connection.
            using (IDbConnection Connection = new SqlConnection(ConnectionString))
            {
                Connection.Open();
                int orderID = Convert.ToInt32(value.key.ToString());

                //Execute this code to reflect the changes into the database.
                Connection.Execute(queryStr, new { OrderID = orderID });
            }

            //Add custom logic here if needed and remove above method.
        }
        /// <summary>
        /// Batchupdate (Insert, Update and Delete) a collection of data items from the data collection.
        /// </summary>
        /// <param name="CRUDModel<T>">The set of information along with details about the CRUD actions to be executed from the database.</param>
        /// <returns>Returns a JsonResult containing the processed data.</returns>
        public JsonResult BatchUpdate(CRUDModel<Orders> value)
        {
            if (value.changed != null && value.changed.Count > 0)
            {
                foreach (Orders Record in (IEnumerable<Orders>)value.changed)
                {
                    //Create query to update the changes into the database by accessing its properties.
                    string queryStr = "UPDATE Orders SET CustomerID = @CustomerID, Freight = @Freight, ShipCity = @ShipCity, EmployeeID = @EmployeeID WHERE OrderID = @OrderID";

                    //Create SQL connection.
                    using (IDbConnection Connection = new SqlConnection(ConnectionString))
                    {
                        Connection.Open();

                        //Execute this code to reflect the changes into the database.
                        Connection.Execute(queryStr, Record);
                    }
                    //Add custom logic here if needed and remove above method.
                }
            }
            if (value.added != null && value.added.Count > 0)
            {
                foreach (Orders Record in (IEnumerable<Orders>)value.added)
                {
                    //Create query to insert the specific into the database by accessing its properties.
                    string queryStr = "INSERT INTO Orders (CustomerID, Freight, ShipCity, EmployeeID) VALUES (@CustomerID, @Freight, @ShipCity, @EmployeeID)";

                    //Create SQL connection.
                    using (IDbConnection Connection = new SqlConnection(ConnectionString))
                    {
                        Connection.Open();

                        //Execute this code to reflect the changes into the database.
                        Connection.Execute(queryStr, Record);
                    }
                    //Add custom logic here if needed and remove above method.
                }
            }
            if (value.deleted != null && value.deleted.Count > 0)
            {
                foreach (Orders Record in (IEnumerable<Orders>)value.deleted)
                {
                    //Create query to remove the specific from database by passing the primary key column value.
                    string queryStr = "DELETE FROM Orders WHERE OrderID = @OrderID";

                    //Create SQL connection.
                    using (IDbConnection Connection = new SqlConnection(ConnectionString))
                    {
                        Connection.Open();

                        //Execute this code to reflect the changes into the database.
                        Connection.Execute(queryStr, new { OrderID = Record.OrderID });
                    }
                    //Add custom logic here if needed and remove above method.
                }
            }
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        #region Models
        /// <summary>
        /// Represents the orders model mapped to the database table.
        /// </summary>
        public class Orders
        {
            public int? OrderID { get; set; }
            public string CustomerID { get; set; }
            public int? EmployeeID { get; set; }
            public decimal? Freight { get; set; }
            public string ShipCity { get; set; }
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
        #endregion
    }
}