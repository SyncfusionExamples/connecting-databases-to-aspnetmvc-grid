using Npgsql;
using Syncfusion.EJ2.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web.Http;

namespace Grid_PostgreSQL.Controllers
{
    public class GridController : ApiController
    {
        private readonly string ConnectionString = "<Enter a valid connection string>";

        [HttpPost]
        public IHttpActionResult UrlDataSource(DataManagerRequest DataManagerRequest)
        {
            IQueryable<Orders> dataSource = GetOrderData().AsQueryable(); // Convert List to IQueryable
            QueryableOperation queryableOperation = new QueryableOperation();

            if (DataManagerRequest.Search?.Count > 0)
            {
                dataSource = queryableOperation.PerformSearching(dataSource, DataManagerRequest.Search);
            }

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

            if (DataManagerRequest.Sorted?.Count > 0)
            {
                dataSource = queryableOperation.PerformSorting(dataSource, DataManagerRequest.Sorted);
            }

            int totalRecordsCount = dataSource.Count();

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

        [HttpGet]
        public List<Orders> GetOrderData()
        {
            // Define the SQL query to fetch all orders from the "Orders" table, ordered by OrderID.
            string query = "SELECT * FROM public.\"Orders\" ORDER BY \"OrderID\"";

            // Establish a connection to the PostgreSQL database using the connection string.
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                // Open the database connection.
                connection.Open();

                // Create a command object to execute the SQL query.
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    // Use a data adapter to fetch data from the database into a DataTable.
                    using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();

                        // Fill the DataTable with the query results.
                        dataAdapter.Fill(dataTable);

                        // Convert the DataTable rows into a list of Orders objects.
                        List<Orders> dataSource = (from DataRow data in dataTable.Rows
                                                   select new Orders
                                                   {
                                                       // Assign values from the DataTable to the Orders object properties.
                                                       OrderID = data["OrderID"] != DBNull.Value ? Convert.ToInt32(data["OrderID"]) : (int?)null,
                                                       CustomerID = data["CustomerID"] != DBNull.Value ? data["CustomerID"].ToString() : null,
                                                       EmployeeID = data["EmployeeID"] != DBNull.Value ? Convert.ToInt32(data["EmployeeID"]) : (int?)null,
                                                       ShipCity = data["ShipCity"] != DBNull.Value ? data["ShipCity"].ToString() : null,
                                                       Freight = data["Freight"] != DBNull.Value ? Convert.ToDecimal(data["Freight"]) : (decimal?)null,
                                                   }).ToList();

                        // Return the list of orders.
                        return dataSource;
                    }
                }
            }
        }

        /// <summary>
        /// Inserts a new order record into the "Orders" table in the PostgreSQL database.
        /// </summary>
        /// <param name="value">The CRUDModel containing the order details to be inserted.</param>
        /// <returns>Returns an HTTP response indicating success or failure.</returns>
        [HttpPost]
        [Route("api/Grid/Insert")]
        public IHttpActionResult Insert(CRUDModel<Orders> value)
        {
            // Check if the received request data is null or invalid.
            if (value?.value == null)
                return BadRequest("Invalid request: No data received.");

            // Establish a connection to the PostgreSQL database.
            using (NpgsqlConnection Connection = new NpgsqlConnection(ConnectionString))
            {
                // Open the database connection.
                Connection.Open();

                // Define an SQL query to insert a new order record.
                string queryStr = "INSERT INTO \"Orders\" (\"CustomerID\", \"Freight\", \"ShipCity\", \"EmployeeID\") " +
                                  "VALUES (@CustomerID, @Freight, @ShipCity, @EmployeeID)";

                // Create a command object to execute the insert query.
                using (NpgsqlCommand Command = new NpgsqlCommand(queryStr, Connection))
                {
                    // Add parameters to prevent SQL injection and ensure data integrity.
                    Command.Parameters.AddWithValue("@CustomerID", value.value.CustomerID);
                    Command.Parameters.AddWithValue("@Freight", value.value.Freight ?? (object)DBNull.Value);
                    Command.Parameters.AddWithValue("@ShipCity", value.value.ShipCity);
                    Command.Parameters.AddWithValue("@EmployeeID", value.value.EmployeeID ?? (object)DBNull.Value);

                    // Execute the insert query and get the number of affected rows.
                    int rowsAffected = Command.ExecuteNonQuery();

                    // Return a success response with a message.
                    return Ok(new { message = "Record inserted successfully." });
                }
            }
        }

        /// <summary>
        /// Updates an existing order record in the "Orders" table in the PostgreSQL database.
        /// </summary>
        /// <param name="value">The CRUDModel containing the updated order details.</param>
        /// <returns>Returns an HTTP response indicating success or failure.</returns>
        [HttpPost]
        [Route("api/Grid/Update")]
        public IHttpActionResult Update(CRUDModel<Orders> value)
        {
            // Check if the received request data is null or invalid.
            if (value?.value == null)
                return BadRequest("Invalid request: No data received.");

            // Establish a connection to the PostgreSQL database.
            using (NpgsqlConnection Connection = new NpgsqlConnection(ConnectionString))
            {
                // Open the database connection.
                Connection.Open();

                // Define an SQL query to update an existing order record based on OrderID.
                string queryStr = "UPDATE \"Orders\" SET \"CustomerID\"=@CustomerID, \"Freight\"=@Freight, " +
                                  "\"ShipCity\"=@ShipCity, \"EmployeeID\"=@EmployeeID WHERE \"OrderID\"=@OrderID";

                // Create a command object to execute the update query.
                using (NpgsqlCommand Command = new NpgsqlCommand(queryStr, Connection))
                {
                    // Add parameters to update order details and prevent SQL injection.
                    Command.Parameters.AddWithValue("@OrderID", value.value.OrderID);
                    Command.Parameters.AddWithValue("@CustomerID", value.value.CustomerID);
                    Command.Parameters.AddWithValue("@Freight", value.value.Freight ?? (object)DBNull.Value);
                    Command.Parameters.AddWithValue("@ShipCity", value.value.ShipCity);
                    Command.Parameters.AddWithValue("@EmployeeID", value.value.EmployeeID ?? (object)DBNull.Value);

                    // Execute the update query and get the number of affected rows.
                    int rowsAffected = Command.ExecuteNonQuery();

                    // Return a success response with a message.
                    return Ok(new { message = "Record updated successfully." });
                }
            }
        }

        /// <summary>
        /// Deletes an order record from the "Orders" table in the PostgreSQL database.
        /// </summary>
        /// <param name="value">The CRUDModel containing the OrderID of the record to be deleted.</param>
        /// <returns>Returns an HTTP response indicating success or failure.</returns>
        [HttpPost]
        [Route("api/Grid/Remove")]
        public IHttpActionResult Remove(CRUDModel<Orders> value)
        {
            // Check if the request contains a valid key (OrderID).
            if (value?.key == null)
                return BadRequest("Invalid request: No key received.");

            // Establish a connection to the PostgreSQL database.
            using (NpgsqlConnection Connection = new NpgsqlConnection(ConnectionString))
            {
                // Open the database connection.
                Connection.Open();

                // Define an SQL query to delete a record from the "Orders" table where OrderID matches.
                string queryStr = "DELETE FROM \"Orders\" WHERE \"OrderID\"=@OrderID";

                // Create a command object to execute the delete query.
                using (NpgsqlCommand Command = new NpgsqlCommand(queryStr, Connection))
                {
                    // Add the OrderID parameter to identify the record to be deleted.
                    Command.Parameters.AddWithValue("@OrderID", value.key);

                    // Execute the delete query and get the number of affected rows.
                    int rowsAffected = Command.ExecuteNonQuery();

                    // Return a success response with a message.
                    return Ok(new { message = "Record deleted successfully." });
                }
            }
        }

        /// <summary>
        /// Performs batch update operations (insert, update, and delete) on the "Orders" table in a single transaction.
        /// </summary>
        /// <param name="value">The CRUDModel containing lists of added, changed, and deleted order records.</param>
        /// <returns>Returns an HTTP response indicating the success or failure of the batch operation.</returns>
        [HttpPost]
        [Route("api/Grid/BatchUpdate")]
        public IHttpActionResult BatchUpdate(CRUDModel<Orders> value)
        {
            // Establish a connection to the PostgreSQL database.
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                // Open the database connection.
                connection.Open();

                // Begin a transaction to ensure atomicity of batch operations.
                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    // Process the list of updated records.
                    if (value.changed != null && value.changed.Count > 0)
                    {
                        // Define an SQL query to update records in the "Orders" table.
                        string updateQuery = "UPDATE \"Orders\" SET \"CustomerID\"=@CustomerID, \"Freight\"=@Freight, \"EmployeeID\"=@EmployeeID, \"ShipCity\"=@ShipCity WHERE \"OrderID\"=@OrderID";

                        // Create a command object to execute the update query within the transaction.
                        using (NpgsqlCommand command = new NpgsqlCommand(updateQuery, connection, transaction))
                        {
                            // Iterate through the list of changed records and update each one.
                            foreach (Orders record in value.changed)
                            {
                                // Clear previous parameters to avoid conflicts.
                                command.Parameters.Clear();

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
                        // Define an SQL query to insert new records into the "Orders" table.
                        string insertQuery = "INSERT INTO \"Orders\" (\"CustomerID\", \"Freight\", \"ShipCity\", \"EmployeeID\") VALUES (@CustomerID, @Freight, @ShipCity, @EmployeeID)";

                        // Create a command object to execute the insert query within the transaction.
                        using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection, transaction))
                        {
                            // Iterate through the list of added records and insert each one.
                            foreach (Orders record in value.added)
                            {
                                // Clear previous parameters to avoid conflicts.
                                command.Parameters.Clear();

                                // Add parameters for inserting new order details.
                                command.Parameters.AddWithValue("@CustomerID", record.CustomerID);
                                command.Parameters.AddWithValue("@Freight", record.Freight ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@EmployeeID", record.EmployeeID ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@ShipCity", record.ShipCity);

                                // Execute the insert query for the current record.
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    // Process the list of deleted records.
                    if (value.deleted != null && value.deleted.Count > 0)
                    {
                        // Define an SQL query to delete records from the "Orders" table based on OrderID.
                        string deleteQuery = "DELETE FROM \"Orders\" WHERE \"OrderID\"=@OrderID";

                        // Create a command object to execute the delete query within the transaction.
                        using (NpgsqlCommand command = new NpgsqlCommand(deleteQuery, connection, transaction))
                        {
                            // Iterate through the list of deleted records and remove each one.
                            foreach (Orders record in value.deleted)
                            {
                                // Clear previous parameters to avoid conflicts.
                                command.Parameters.Clear();

                                // Add the OrderID parameter to delete the corresponding record.
                                command.Parameters.AddWithValue("@OrderID", record.OrderID);

                                // Execute the delete query for the current record.
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    // Commit the transaction to apply all changes.
                    transaction.Commit();
                }
            }

            // Return a success message after completing all batch operations.
            return Ok(new { message = "Batch update completed successfully." });
        }

        public class Orders
        {
            [Key]
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
    }
}
