using Microsoft.Data.Sqlite;
using Syncfusion.EJ2.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Grid_SQLite.Controllers
{
    public class GridController : Controller
    {
        string ConnectionString = @"Data Source=C:\sqlite\gui\ordermanagementdb.db;";


        /// <summary>
        /// Processes the DataManager request to perform searching, filtering, sorting, and paging operations.
        /// </summary>
        /// <param name="DataManagerRequest">Contains the details of the data operation requested.</param>
        /// <returns>Returns a JSON object along with the total record count.</returns>
        public object UrlDataSource(DataManagerRequest DataManagerRequest)
        {
            // Retrieve data from the data source (e.g., database).
            IQueryable<Orders> DataSource = GetOrderData().AsQueryable();

            // Initialize QueryableOperation instance.
            QueryableOperation queryableOperation = new QueryableOperation();

            // Handling searching operation.
            if (DataManagerRequest.Search != null && DataManagerRequest.Search.Count > 0)
            {
                DataSource = queryableOperation.PerformSearching(DataSource, DataManagerRequest.Search);
                //Add custom logic here if needed and remove above method.
            }

            // Handling filtering operation.
            if (DataManagerRequest.Where != null && DataManagerRequest.Where.Count > 0)
            {
                foreach (WhereFilter condition in DataManagerRequest.Where)
                {
                    foreach (WhereFilter predicate in condition.predicates)
                    {
                        DataSource = queryableOperation.PerformFiltering(DataSource, DataManagerRequest.Where, predicate.Operator);
                        //Add custom logic here if needed and remove above method.
                    }
                }
            }

            // Handling sorting operation.
            if (DataManagerRequest.Sorted != null && DataManagerRequest.Sorted.Count > 0)
            {
                DataSource = queryableOperation.PerformSorting(DataSource, DataManagerRequest.Sorted);
                //Add custom logic here if needed and remove above method.
            }

            // Get the total count of records.
            int totalRecordsCount = DataSource.Count();

            // Handling paging operation.
            if (DataManagerRequest.Skip != 0)
            {
                DataSource = queryableOperation.PerformSkip(DataSource, DataManagerRequest.Skip);
                //Add custom logic here if needed and remove above method.
            }
            if (DataManagerRequest.Take != 0)
            {
                DataSource = queryableOperation.PerformTake(DataSource, DataManagerRequest.Take);
                //Add custom logic here if needed and remove above method.
            }

            // Return data based on the request.
            return Json(new { result = DataSource, count = totalRecordsCount }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Retrieves the order data from the database.
        /// </summary>
        /// <returns>Returns a list of orders fetched from the database.</returns>
        public List<Orders> GetOrderData()
        {
            // SQL query to fetch all records from the orders table and sort them by OrderID.
            string queryStr = "SELECT * FROM Orders ORDER BY OrderID;";

            // Creates an empty list to store the retrieved orders from the database.
            List<Orders> DataSource = new List<Orders>();
            // Establishes a connection to the SQLite database using the provided connection string.
            SqliteConnection Connection = new SqliteConnection(ConnectionString);

            // Opens the database connection to enable SQL queries.
            Connection.Open();

            //Using SqliteCommand and query create connection with database.
            SqliteCommand Command = new SqliteCommand(queryStr, Connection);

            // Execute the SQLite command and retrieve data using SqliteDataReader.
            using (SqliteDataReader reader = Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Orders order = new Orders
                    {
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        CustomerID = reader.GetString(reader.GetOrdinal("CustomerID")),
                        EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                        ShipCity = reader.GetString(reader.GetOrdinal("ShipCity")),
                        Freight = reader.GetDecimal(reader.GetOrdinal("Freight"))
                    };
                    DataSource.Add(order);
                }
            }
            return DataSource;
        }

        public void Insert(Orders value)
        {
            //Create query to insert the specific into the database by accessing its properties.
            string queryStr = $"Insert into Orders(CustomerID,Freight,ShipCity,EmployeeID) values('{value.CustomerID}','{value.Freight}','{value.ShipCity}','{value.EmployeeID}')";

            // Establishes a connection to the SQLite database using the provided connection string.
            SqliteConnection Connection = new SqliteConnection(ConnectionString);

            // Opens the database connection to execute queries.
            Connection.Open();

            //Execute the SQLite command.
            SqliteCommand Command = new SqliteCommand(queryStr, Connection);

            //Execute this code to reflect the changes into the database.
            Command.ExecuteNonQuery();

            // Close the database connection after executing the query.
            Connection.Close();

            //Add custom logic here if needed and remove above method.
        }

        /// <summary>
        /// Update a existing data item from the data collection.
        /// </summary>
        /// <param name="value">It contains the updated record detail which is need to be updated.</param>
        /// <returns>Returns void.</returns>
        
        public void Update(Orders value)
        {
            //Create query to update the changes into the database by accessing its properties.
            string queryStr = $"Update Orders set CustomerID='{value.CustomerID}', Freight='{value.Freight}',EmployeeID='{value.EmployeeID}',ShipCity='{value.ShipCity}' where OrderID='{value.OrderID}'";

            // Establishes a connection to the SQLite database using the provided connection string.
            SqliteConnection Connection = new SqliteConnection(ConnectionString);

            // Opens the database connection to execute queries.
            Connection.Open();

            //Execute the SQLite command.
            SqliteCommand Command = new SqliteCommand(queryStr, Connection);

            //Execute this code to reflect the changes into the database.
            Command.ExecuteNonQuery();

            // Close the database connection after executing the query.
            Connection.Close();

            //Add custom logic here if needed and remove above method.
        }
        public void Remove(CRUDModel<Orders> value)
        {
            //Create query to remove the specific from database by passing the primary key column value.
            string queryStr = $"Delete from Orders where OrderID={value.key}";

            // Establishes a connection to the SQLite database using the provided connection string.
            SqliteConnection Connection = new SqliteConnection(ConnectionString);

            // Opens the database connection to execute queries.
            Connection.Open();

            //Execute the SQLite command.
            SqliteCommand Command = new SqliteCommand(queryStr, Connection);

            //Execute this code to reflect the changes into the database.
            Command.ExecuteNonQuery();

            // Close the database connection after executing the query.
            Connection.Close();

            //Add custom logic here if needed and remove above method.
        }

        public JsonResult BatchUpdate(CRUDModel<Orders> value)
        {
            using (SqliteConnection connection = new SqliteConnection(ConnectionString))
            {
                // Open the database connection.
                connection.Open();

                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        // Process updated records if any.
                        if (value.changed != null && value.changed.Count > 0)
                        {
                            command.CommandText = "UPDATE Orders SET CustomerID=@CustomerID, Freight=@Freight, EmployeeID=@EmployeeID, ShipCity=@ShipCity WHERE OrderID=@OrderID";
                            foreach (Orders record in value.changed)
                            {
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@CustomerID", record.CustomerID);
                                command.Parameters.AddWithValue("@Freight", record.Freight);
                                command.Parameters.AddWithValue("@EmployeeID", record.EmployeeID);
                                command.Parameters.AddWithValue("@ShipCity", record.ShipCity);
                                command.Parameters.AddWithValue("@OrderID", record.OrderID);
                                command.ExecuteNonQuery();
                            }
                        }

                        // Process inserted records if any.
                        if (value.added != null && value.added.Count > 0)
                        {
                            command.CommandText = "INSERT INTO Orders (CustomerID, Freight, ShipCity, EmployeeID) VALUES (@CustomerID, @Freight, @ShipCity, @EmployeeID)";
                            foreach (Orders record in value.added)
                            {
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@CustomerID", record.CustomerID);
                                command.Parameters.AddWithValue("@Freight", record.Freight);
                                command.Parameters.AddWithValue("@ShipCity", record.ShipCity);
                                command.Parameters.AddWithValue("@EmployeeID", record.EmployeeID);
                                command.ExecuteNonQuery();
                            }
                        }

                        // Process deleted records if any.
                        if (value.deleted != null && value.deleted.Count > 0)
                        {
                            command.CommandText = "DELETE FROM Orders WHERE OrderID=@OrderID";
                            foreach (Orders record in value.deleted)
                            {
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@OrderID", record.OrderID);
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    // Commit the transaction.
                    transaction.Commit();
                }
            }

            return Json(new { success = true, message = "Batch update successful.", data = value }, JsonRequestBehavior.AllowGet);
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
            public int? OrderID { get; set; }
            public string CustomerID { get; set; }
            public int? EmployeeID { get; set; }
            public decimal? Freight { get; set; }
            public string ShipCity { get; set; }
        }
    }
}