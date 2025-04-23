using MySql.Data.MySqlClient;
using Syncfusion.EJ2.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Results;

namespace Grid_MYSQL.Controllers
{
   
    public class GridController : ApiController
    {
        public class Orders
        {
            [Key]
            public int? OrderID { get; set; }
            public string CustomerID { get; set; }
            public int? EmployeeID { get; set; }
            public decimal? Freight { get; set; }
            public string ShipCity { get; set; }
            public DateTime OrderDate { get; set; }
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

        private string ConnectionString = @"<Enter a valid connection string>";

        /// <summary>
        /// Processes the DataManager request to perform sorting operation.
        /// </summary>
        /// <param name="DataManagerRequest">Contains the details of the data operation requested.</param>
        /// <returns>Returns a JSON object with the filtered, sorted, and paginated data along with the total record count.</returns>
        [HttpPost]
        public IHttpActionResult UrlDataSource(DataManagerRequest DataManagerRequest)
        {
            // Retrieve data from the data source (e.g., database).
            IQueryable<Orders> dataSource = GetOrderData().AsQueryable();

            // Initialize QueryableOperation instance.
            QueryableOperation queryableOperation = new QueryableOperation();

            // Handle searching operation.
            if (DataManagerRequest.Search?.Count > 0)
            {
                dataSource = queryableOperation.PerformSearching(dataSource, DataManagerRequest.Search);
            }

            // Handle filtering operation.
            if (DataManagerRequest.Where?.Count > 0)
            {
                foreach (WhereFilter condition in DataManagerRequest.Where)
                {
                    foreach (WhereFilter predicate in condition.predicates)
                    {
                        dataSource = queryableOperation.PerformFiltering(dataSource, DataManagerRequest.Where, predicate.Operator);
                    }
                }
            }

            // Handle sorting operation.
            if (DataManagerRequest.Sorted?.Count > 0)
            {
                dataSource = queryableOperation.PerformSorting(dataSource, DataManagerRequest.Sorted);
            }

            int totalRecordsCount = dataSource.Count();

            // Handle paging operation.
            if (DataManagerRequest.Skip > 0)
            {
                dataSource = queryableOperation.PerformSkip(dataSource, DataManagerRequest.Skip);
            }
            if (DataManagerRequest.Take > 0)
            {
                dataSource = queryableOperation.PerformTake(dataSource, DataManagerRequest.Take);
            }

            return Ok(new { result = dataSource, count = totalRecordsCount });
        }

        /// <summary>
        /// Inserts a new order record into the database using parameterized queries.
        /// </summary>
        /// <param name="newRecord">It contains the new record detail which is need to be inserted.</param>
        /// <returns>Returns a JSON result indicating success.</returns>
        [HttpPost]
        [Route("api/Grid/Insert")]
        public IHttpActionResult Insert(CRUDModel<Orders> newRecord)
        {
            // Check if the request data is null or invalid.
            if (newRecord?.value == null)
                return BadRequest("Invalid request: No data received.");

            // Establish a connection to the MySQL database.
            using (MySqlConnection Connection = new MySqlConnection(ConnectionString))
            {
                // Open the database connection.
                Connection.Open();

                // Define the SQL query to insert a new record into the orders table.
                string queryStr = "INSERT INTO Orders (CustomerID, Freight, ShipCity, EmployeeID, OrderDate) VALUES (@CustomerID, @Freight, @ShipCity, @EmployeeID, @OrderDate)";

                // Create a MySQL command with the query and connection.
                using (MySqlCommand Command = new MySqlCommand(queryStr, Connection))
                {
                    // Add parameters to prevent SQL injection and handle null values.
                    Command.Parameters.AddWithValue("@CustomerID", newRecord.value.CustomerID);
                    Command.Parameters.AddWithValue("@Freight", newRecord.value.Freight ?? (object)DBNull.Value);
                    Command.Parameters.AddWithValue("@ShipCity", newRecord.value.ShipCity);
                    Command.Parameters.AddWithValue("@EmployeeID", newRecord.value.EmployeeID ?? (object)DBNull.Value);
                    Command.Parameters.AddWithValue("@OrderDate", DateTime.Now);  // Explicitly setting OrderDate.
                    // Execute the SQL query and insert the record.
                    int rowsAffected = Command.ExecuteNonQuery();

                    // Return a success message if the insertion is successful.
                    return Ok(new { message = "Record inserted successfully" });
                }
            }
        }

        /// <summary>
        /// Updates an existing order record in the database using parameterized queries.
        /// </summary>
        /// <param name="value">Contains the details of the order that needs to be updated.</param>
        /// <returns>Returns a JSON result indicating success or failure.</returns>
        [HttpPost]
        [Route("api/Grid/Update")]
        public IHttpActionResult Update(CRUDModel<Orders> value)
        {
            // Check if the request data is null or invalid.
            if (value?.value == null)
                return BadRequest("Invalid request: No data received.");

            // Establish a connection to the MySQL database.
            using (MySqlConnection Connection = new MySqlConnection(ConnectionString))
            {
                // Open the database connection.
                Connection.Open();

                // Define the SQL query to update an existing record in the orders table.
                string queryStr = "UPDATE Orders SET CustomerID=@CustomerID, Freight=@Freight, ShipCity=@ShipCity, EmployeeID=@EmployeeID WHERE OrderID=@OrderID";

                // Create a MySQL command with the query and connection.
                using (MySqlCommand Command = new MySqlCommand(queryStr, Connection))
                {
                    // Add parameters to prevent SQL injection and handle null values.
                    Command.Parameters.AddWithValue("@OrderID", value.value.OrderID);
                    Command.Parameters.AddWithValue("@CustomerID", value.value.CustomerID);
                    Command.Parameters.AddWithValue("@Freight", value.value.Freight ?? (object)DBNull.Value);
                    Command.Parameters.AddWithValue("@ShipCity", value.value.ShipCity);
                    Command.Parameters.AddWithValue("@EmployeeID", value.value.EmployeeID ?? (object)DBNull.Value);
                    Command.Parameters.AddWithValue("@OrderDate", DateTime.Now);

                    // Execute the SQL query and update the record.
                    int rowsAffected = Command.ExecuteNonQuery();

                    // Return a success message if the update is successful.
                    return Ok(new { message = "Record updated successfully" });
                }
            }
        }

        /// <summary>
        /// Deletes an existing order record from the database using the specified OrderID.
        /// </summary>
        /// <param name="value">Contains the key (OrderID) of the order to be deleted.</param>
        /// <returns>Returns a JSON result indicating success or failure.</returns>
        [HttpPost]
        [Route("api/Grid/Remove")]
        public IHttpActionResult Remove(CRUDModel<Orders> value)
        {
            // Check if the request contains a valid OrderID.
            if (value?.key == null)
                return BadRequest("Invalid request: No key received.");

            // Establish a connection to the MySQL database.
            using (MySqlConnection Connection = new MySqlConnection(ConnectionString))
            {
                // Open the database connection.
                Connection.Open();

                // Define the SQL query to delete the order record with the specified OrderID.
                string queryStr = "DELETE FROM Orders WHERE OrderID=@OrderID";

                // Create a MySQL command with the query and connection.
                using (MySqlCommand Command = new MySqlCommand(queryStr, Connection))
                {
                    // Add the OrderID parameter to the query to prevent SQL injection.
                    Command.Parameters.AddWithValue("@OrderID", value.key);

                    // Execute the SQL query and delete the record.
                    int rowsAffected = Command.ExecuteNonQuery();

                    // Return a success message if the deletion is successful.
                    return Ok(new { message = "Record deleted successfully" });
                }
            }
        }

        [HttpPost]
        [Route("api/Grid/BatchUpdate")]
        public IHttpActionResult BatchUpdate([FromBody] CRUDModel<Orders> value)
        {
            // Establish a connection to the MySQL database.
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                // Open the database connection.
                connection.Open();

                // Process the list of updated records.
                if (value.changed != null && value.changed.Count > 0)
                {
                    // Define an SQL query to update records in the Orders table.
                    string updateQuery = "UPDATE Orders SET CustomerID=@CustomerID, Freight=@Freight, EmployeeID=@EmployeeID, ShipCity=@ShipCity WHERE OrderID=@OrderID";

                    // Create a MySQL command object to execute the update query.
                    using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                    {
                        // Iterate through the list of changed records and update each one.
                        foreach (Orders record in value.changed)
                        {
                            // Add parameters for updating the order details.
                            command.Parameters.AddWithValue("@OrderID", record.OrderID);
                            command.Parameters.AddWithValue("@CustomerID", record.CustomerID);
                            command.Parameters.AddWithValue("@Freight", record.Freight ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@EmployeeID", record.EmployeeID ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@ShipCity", record.ShipCity);

                            // Execute the update query for the current record.
                            command.ExecuteNonQuery();
                        }
                    }
                }

                // Process the list of newly added records.
                if (value.added != null && value.added.Count > 0)
                {
                    // Define an SQL query to insert new records into the Orders table.
                    string insertQuery = "INSERT INTO Orders (CustomerID, Freight, ShipCity, EmployeeID, OrderDate) VALUES (@CustomerID, @Freight, @ShipCity, @EmployeeID, @OrderDate)";

                    // Create a MySQL command object to execute the insert query.
                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        // Iterate through the list of added records and insert each one.
                        foreach (Orders record in value.added)
                        {
                            // Add parameters for inserting new order details.
                            command.Parameters.AddWithValue("@CustomerID", record.CustomerID);
                            command.Parameters.AddWithValue("@Freight", record.Freight ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@EmployeeID", record.EmployeeID ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@ShipCity", record.ShipCity);
                            command.Parameters.AddWithValue("@OrderDate", DateTime.Now); // Set current date as the order date.

                            // Execute the insert query for the current record.
                            command.ExecuteNonQuery();
                        }
                    }
                }

                // Process the list of deleted records.
                if (value.deleted != null && value.deleted.Count > 0)
                {
                    // Define an SQL query to delete records from the Orders table based on OrderID.
                    string deleteQuery = "DELETE FROM Orders WHERE OrderID=@OrderID";

                    // Create a MySQL command object to execute the delete query.
                    using (MySqlCommand command = new MySqlCommand(deleteQuery, connection))
                    {
                        // Iterate through the list of deleted records and remove each one.
                        foreach (Orders record in value.deleted)
                        {
                            // Add the OrderID parameter to delete the corresponding record.
                            command.Parameters.AddWithValue("@OrderID", record.OrderID);

                            // Execute the delete query for the current record.
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

            // Return a success message after completing all batch operations.
            return Ok(new { message = "Batch update completed successfully." });
        }

        [HttpGet]
        public List<Orders> GetOrderData()
        {
            List<Orders> orders = new List<Orders>();
            using (MySqlConnection sqlConnection = new MySqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM orders ORDER BY OrderID";
                using (MySqlCommand sqlCommand = new MySqlCommand(query, sqlConnection))
                {
                    sqlConnection.Open();
                    using (MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCommand))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        orders = (from DataRow row in dataTable.Rows
                                  select new Orders()
                                  {
                                      OrderID = row["OrderID"] != DBNull.Value ? Convert.ToInt32(row["OrderID"]) : (int?)null,
                                      CustomerID = row["CustomerID"]?.ToString(),
                                      EmployeeID = row["EmployeeID"] != DBNull.Value ? Convert.ToInt32(row["EmployeeID"]) : (int?)null,
                                      ShipCity = row["ShipCity"]?.ToString(),
                                      Freight = row["Freight"] != DBNull.Value ? Convert.ToDecimal(row["Freight"]) : (decimal?)null
                                  }).ToList();
                    }
                }
            }
            return orders;
        }
    }
}
