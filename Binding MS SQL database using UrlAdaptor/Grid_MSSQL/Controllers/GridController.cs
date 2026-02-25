using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Syncfusion.EJ2.Base;
using Syncfusion.EJ2.Linq;

namespace Grid_MSSQL.Controllers
{
    public class GridController : Controller
    {
        /// <summary>
        /// Connection string for the database.
        /// </summary>
        private readonly string ConnectionString = @"<Enter a valid connection string>";

        /// <summary>
        /// Processes the DataManager request to perform searching, filtering, sorting, and paging operations.
        /// </summary>
        /// <param name="DataManagerRequest">Contains the details of the data operation requested.</param>
        /// <returns>Returns a JSON object with the filtered, sorted, and paginated data along with the total record count.</returns>
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
        /// Inserts a new order record into the database using parameterized queries.
        /// </summary>
        /// <param name="model">Contains the details of the order to be inserted.</param>
        /// <returns>Returns a JSON result indicating success.</returns>
        public JsonResult Insert(CRUDModel<Orders> model)
        {
            using (sqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                // Define the SQL query to insert a new order record using parameters.
                string query = "INSERT INTO Orders (CustomerID, Freight, ShipCity, EmployeeID) VALUES (@CustomerID, @Freight, @ShipCity, @EmployeeID)";

                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    // Add parameters to prevent SQL injection and handle null values.
                    sqlCommand.Parameters.AddWithValue("@CustomerID", model.value.CustomerID ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Freight", model.value.Freight ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ShipCity", model.value.ShipCity ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@EmployeeID", model.value.EmployeeID ?? (object)DBNull.Value);

                    // Open the database connection and execute the command.
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
            }

            // Return a JSON response indicating success.
            return Json(new { success = true });
        }

        /// <summary>
        /// Update a existing data item from the data collection.
        /// </summary>
        /// <param name="model">It contains the updated record detail which is need to be updated.</param>
        /// <returns>Returns a JSON result indicating success.</returns>
        public JsonResult Update(CRUDModel<Orders> model)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                // Define the SQL query to update the order details based on OrderID.
                string query = "UPDATE Orders SET CustomerID=@CustomerID, Freight=@Freight, EmployeeID=@EmployeeID, ShipCity=@ShipCity WHERE OrderID=@OrderID";

                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    // Add parameters to ensure data integrity and prevent SQL injection.
                    sqlCommand.Parameters.AddWithValue("@CustomerID", model.value.CustomerID ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@Freight", model.value.Freight ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@ShipCity", model.value.ShipCity ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@EmployeeID", model.value.EmployeeID ?? (object)DBNull.Value);
                    sqlCommand.Parameters.AddWithValue("@OrderID", model.value.OrderID ?? (object)DBNull.Value);

                    // Open the database connection and execute the update command.
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
            }

            // Return a JSON response indicating success.
            return Json(new { success = true });
        }

        /// <summary>
        /// Remove a specific data item from the data collection.
        /// </summary>
        /// <param name="value">It contains the specific record detail which is need to be removed.</param>
        /// <returns>Returns a JSON result indicating success.</returns>
        public JsonResult Remove(CRUDModel<Orders> model)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                // Define the SQL query to delete the order based on OrderID.
                string query = "DELETE FROM Orders WHERE OrderID=@OrderID";

                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    // Add parameter to ensure data integrity and prevent SQL injection.
                    sqlCommand.Parameters.AddWithValue("@OrderID", model.key ?? (object)DBNull.Value);

                    // Open the database connection and execute the delete command.
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
            }

            // Return a JSON response indicating success.
            return Json(new { success = true });
        }

        /// <summary>
        /// Retrieves order data from the database.
        /// </summary>
        /// <returns>Returns a list of orders fetched from the database.</returns>
        private List<Orders> GetOrderData()
        {
            // SQL query to select all records from the orders table, sorted by OrderID.
            string query = "SELECT * FROM dbo.Orders ORDER BY OrderID;";

            // List to store the retrieved order data.
            List<Orders> orders = new List<Orders>();

            // Using block to ensure proper disposal of the SQL connection.
            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                // Open the database connection.
                sqlConnection.Open();

                // Using block to ensure proper disposal of the SQL command and data adapter.
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    // DataTable to store the query result.
                    DataTable dataTable = new DataTable();

                    // Fill the DataTable with data from the database.
                    dataAdapter.Fill(dataTable);

                    // Convert DataTable rows into a list of orders objects.
                    orders = (from DataRow row in dataTable.Rows
                              select new Orders
                              {
                                  OrderID = Convert.ToInt32(row["OrderID"]),
                                  CustomerID = row["CustomerID"].ToString(),
                                  EmployeeID = Convert.IsDBNull(row["EmployeeID"]) ? (int?)null : Convert.ToInt32(row["EmployeeID"]),
                                  ShipCity = row["ShipCity"].ToString(),
                                  Freight = Convert.ToDecimal(row["Freight"])
                              }).ToList();
                }
            }

            // Return the list of orders.
            return orders;
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