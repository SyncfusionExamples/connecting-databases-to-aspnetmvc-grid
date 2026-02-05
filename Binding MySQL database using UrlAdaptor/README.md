---
layout: post
title: ASP.NET MVC5 Data Grid connected to MySQL via LINQ2DB | Syncfusion
description: Bind MySQL database data to ASP.NET MVC5 Data Grid using LINQ2DB with complete CRUD, filtering, sorting, paging, and advanced data operations using UrlAdaptor and CustomAdaptor patterns.
platform: ASP.NET MVC5
control: DataGrid (EJ2 Grid)
documentation: ug
---

# Connecting MySQL to ASP.NET MVC5 Data Grid Using LINQ2DB

The [Syncfusion<sup style="font-size:70%">&reg;</sup> EJ2 ASP.NET MVC5 DataGrid](https://www.syncfusion.com/aspnet-mvc-components/mvc-grid) supports binding data from a MySQL database. This documentation demonstrates how to integrate MySQL with Syncfusion EJ2 Grid using LINQ2DB (Light-weight ORM) for data operations with both **UrlAdaptor** and **CustomAdaptor** approaches.

**What is LINQ2DB?**

LINQ2DB is a lightweight Object-Relational Mapping (ORM) library for .NET that simplifies database operations. It provides a bridge between C# code and databases like MySQL, enabling type-safe queries without the overhead of heavier frameworks.

**Key Benefits of LINQ2DB for MySQL and Syncfusion Grid Integration**

- **Lightweight Performance**: Minimal overhead compared to Entity Framework, ideal for web applications requiring fast database access.
- **LINQ Support**: Use familiar LINQ syntax for database queries instead of raw SQL strings.
- **Type Safety**: Strong typing reduces runtime errors and provides IntelliSense support.
- **Built-in Security**: Automatic parameterization prevents SQL injection attacks.
- **MySQL-Specific**: Full support for MySQL 5.7+ and MySQL 8.0+ with proper collation and character encoding handling.
- **Minimal Configuration**: Simple setup with straightforward connection string management.
- **Compatibility with Syncfusion DataManager**: Works seamlessly with Syncfusion EJ2 Grid's built-in data operations (filtering, sorting, paging, searching).

**What is the Difference Between UrlAdaptor and CustomAdaptor?**

| Aspect | UrlAdaptor | CustomAdaptor |
|--------|-----------|---------------|
| **Data Processing** | Server-side | Custom server-side logic |
| **HTTP Method** | POST requests to controller actions | Custom POST endpoints for batch operations |
| **Use Case** | Standard CRUD operations | Complex business logic, batch processing, custom validation |
| **Built-in DataManager** | Relies on Syncfusion's QueryableOperation for filtering/sorting/paging | Manual implementation of query operations |
| **Configuration** | Simple URL mapping | Custom ActionResult methods |

---

## Prerequisites

Ensure the following software and packages are installed before proceeding:

| Software/Package | Version | Purpose |
|-----------------|---------|---------|
| Visual Studio 2022 | 17.0 or later | Development IDE with ASP.NET MVC workload |
| .NET Framework | 4.8.1 or compatible | Runtime framework |
| MySQL Server | 5.7 or later (8.0 recommended) | Database server |
| Syncfusion.EJ2.MVC5 | 32.1.19 or later | EJ2 DataGrid and UI components for ASP.NET MVC5 |
| Syncfusion.EJ2.JavaScript | 32.1.19 or later | Client-side JavaScript for Syncfusion controls |
| Syncfusion.Licensing | 32.1.19 or later | Licensing for Syncfusion components |
| linq2db | 6.1.0 or later | Light-weight ORM for database operations |
| linq2db.MySql | 6.1.0 or later | MySQL provider for LINQ2DB |
| MySqlConnector | 2.5.0 or later | Modern MySQL connector for .NET (recommended over MySql.Data) |
| Microsoft.AspNet.Mvc | 5.3.0 | ASP.NET MVC framework (Note: Default MVC5 template installs 5.2.9; must be updated to 5.3.0) |

**Important Note on Microsoft.AspNet.Mvc Version:**

When creating a new ASP.NET MVC5 project using the Visual Studio template, the default installed version is **5.2.9** refer [wikipedia](https://en.wikipedia.org/wiki/ASP.NET_MVC) for more details. However, Syncfusion packages (version 32.1.19) are built and tested against **5.3.0**. To ensure compatibility and avoid potential runtime errors, you must update the package immediately after project creation.

**How to update Microsoft.AspNet.Mvc to 5.3.0:**

1. Open **Package Manager Console** in Visual Studio.
2. Run the following command:

```powershell
Update-Package Microsoft.AspNet.Mvc -Version 5.3.0
```

This resolves version conflicts and ensures all Syncfusion components function correctly.

---

## Setting Up the MySQL Environment

### Step 1: Create the Database and Table in MySQL

First, the **MySQL database** structure must be created to store transaction records.

**Instructions:**
1. Open MySQL Workbench, MySQL Command Line Client, or any MySQL client.
2. Execute the following SQL script to create the database and table:

Run the following SQL script:

```sql
-- Create database + select it
CREATE DATABASE IF NOT EXISTS transactiondb
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_general_ci;
USE transactiondb;

-- Create table
CREATE TABLE IF NOT EXISTS transactions (
  Id               INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  TransactionId    VARCHAR(50) NOT NULL UNIQUE,
  CustomerId       INT NOT NULL,
  OrderId          INT NULL,
  InvoiceNumber    VARCHAR(50) NULL,
  Description      VARCHAR(500) NULL,
  Amount           DECIMAL(15,2) NOT NULL,
  CurrencyCode     VARCHAR(10) NULL,
  TransactionType  VARCHAR(50) NULL,
  PaymentGateway   VARCHAR(100) NULL,
  CreatedAt        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CompletedAt      DATETIME NULL,
  Status           VARCHAR(50) NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Seed data
INSERT INTO transactions
  (TransactionId, CustomerId, OrderId, InvoiceNumber, Description, Amount, CurrencyCode,
   TransactionType, PaymentGateway, CreatedAt, CompletedAt, Status)
VALUES
('TXN260113001', 1001, 50001, 'INV-2026-001', 'Samsung S25 Ultra', 153399.00, 'INR',
 'SALE', 'Razorpay', '2026-01-13 10:15:30', '2026-01-13 10:16:55', 'SUCCESS'),
('TXN260113002', 1002, 50002, 'INV-2026-002', 'MacBook Pro M4', 224199.00, 'INR',
 'SALE', 'Stripe', '2026-01-13 11:20:10', '2026-01-13 11:21:40', 'SUCCESS');
```

**Key Points:**

- **Character Set**: `utf8mb4` is used for proper UTF-8 support including emojis and international characters.
- **Collation**: `utf8mb4_general_ci` provides case-insensitive comparison suitable for most applications.
- **Engine**: `InnoDB` ensures ACID compliance and transaction support.
- **AUTO_INCREMENT**: The `Id` column auto-increments with each new record.

After executing this script, the transaction records are stored in the `transactions` table within the `transactiondb` database. The database is now ready for integration with the ASP.NET MVC5 application.

---

### Step 2: Create a New ASP.NET MVC5 Project

Before installing NuGet packages, a new ASP.NET MVC5 Web Application must be created.

**Instructions:**

1. Open **Visual Studio 2022**.
2. Click **Create a new project**.
3. Search for **ASP.NET Web Application (.NET Framework)**.
4. Select the template and click **Next**.
5. Configure the project:
   - **Project name**: `MySQLMVC5Grid` (or your preferred name)
   - **Location**: Choose your desired folder
   - **Framework**: Select **.NET Framework 4.8.1**
6. Click **Create**.
7. In the **Create a new ASP.NET Web Application** dialog:
   - Select **MVC** template.
   - Ensure **Authentication** is set to **No Authentication** for simplicity.
   - Click **Create**.

Visual Studio will create the project with the default MVC5 structure, including folders like **Controllers**, **Models**, **Views**, and configuration files. The ASP.NET MVC5 project is now ready for integration with LINQ2DB and Syncfusion components.

---

### Step 3: Install Required NuGet Packages

NuGet packages are software libraries that add functionality to the application. These packages enable LINQ2DB, MySQL connectivity, and Syncfusion Grid integration.

**Method 1: Using Package Manager Console (Recommended)**

1. Open Visual Studio 2022.
2. Navigate to **Tools → NuGet Package Manager → Package Manager Console**.
3. Run the following commands in sequence:

```powershell
Install-Package Syncfusion.EJ2.MVC5 -Version 32.1.19
Install-Package Syncfusion.EJ2.JavaScript -Version 32.1.19
Install-Package Syncfusion.Licensing -Version 32.1.19
Install-Package linq2db -Version 6.1.0
Install-Package linq2db.MySql -Version 6.1.0
Install-Package MySqlConnector -Version 2.5.0
Update-Package Microsoft.AspNet.Mvc -Version 5.3.0
```

**Important:** The last command updates Microsoft.AspNet.Mvc from the default 5.2.9 to 5.3.0 for compatibility with Syncfusion packages.

**Method 2: Using NuGet Package Manager UI**

1. Open **Visual Studio 2022 → Tools → NuGet Package Manager → Manage NuGet Packages for Solution**.
2. Search for and install each package individually:
   - **Syncfusion.EJ2.MVC5** (version 32.1.19)
   - **Syncfusion.EJ2.JavaScript** (version 32.1.19)
   - **Syncfusion.Licensing** (version 32.1.19)
   - **linq2db** (version 6.1.0)
   - **linq2db.MySql** (version 6.1.0)
   - **MySqlConnector** (version 2.5.0)
3. Update **Microsoft.AspNet.Mvc** to version **5.3.0**.

All required packages are now installed. Verify the installation by checking the `packages.config` file in the project root.

---

### Step 4: Create the Data Model

A data model is a C# class that represents the structure of a database table. This model defines the properties that correspond to the columns in the `transactions` table.

**Instructions:**

1. In the **Solution Explorer**, right-click on the **Models** folder.
2. Select **Add → New Item**.
3. Choose **Class** and name it **Transactions.cs**.
4. Replace the default code with the following:

```csharp
using LinqToDB.Mapping;
using System;

namespace MySQLMVC5Grid.Models
{
    /// <summary>
    /// Represents a financial transaction record in the MySQL transactiondb database.
    /// This class uses LinqToDB attributes for database mapping.
    /// </summary>
    [Table("transactions")]
    public class Transactions
    {
        /// <summary>
        /// Primary key. Auto-incremented by MySQL with AUTO_INCREMENT.
        /// </summary>
        [PrimaryKey, Identity]
        public int Id { get; set; }

        /// <summary>
        /// Unique transaction identifier (e.g., TXN260113001).
        /// </summary>
        [Column, NotNull]
        public string TransactionId { get; set; }

        /// <summary>
        /// Unique identifier of the customer who initiated the transaction.
        /// </summary>
        [Column, NotNull]
        public int CustomerId { get; set; }

        /// <summary>
        /// Order ID associated with this transaction (nullable if transaction is not order-related).
        /// </summary>
        [Column]
        public int? OrderId { get; set; }

        /// <summary>
        /// Invoice number for accounting and billing purposes.
        /// </summary>
        [Column]
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Detailed description of the transaction or purchased items.
        /// </summary>
        [Column]
        public string Description { get; set; }

        /// <summary>
        /// Transaction amount in decimal format with 2 decimal places.
        /// Example: 153399.00 for 153,399 rupees.
        /// </summary>
        [Column, NotNull]
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency code (e.g., INR, USD, EUR).
        /// </summary>
        [Column]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Type of transaction (SALE, REFUND, ADJUSTMENT, etc.).
        /// </summary>
        [Column]
        public string TransactionType { get; set; }

        /// <summary>
        /// Payment gateway used to process the transaction (Razorpay, Stripe, PayPal, etc.).
        /// </summary>
        [Column]
        public string PaymentGateway { get; set; }

        /// <summary>
        /// Timestamp when the transaction was created (set by MySQL DEFAULT CURRENT_TIMESTAMP).
        /// </summary>
        [Column, NotNull]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the transaction was completed or settled (nullable).
        /// </summary>
        [Column]
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Current status of the transaction (SUCCESS, PENDING, FAILED, PROCESSING).
        /// </summary>
        [Column]
        public string Status { get; set; }
    }
}
```

**Explanation:**

- **`[Table("transactions")]`**: Maps the class to the `transactions` table in the MySQL database (note: lowercase, as MySQL is case-sensitive on Linux).
- **`[PrimaryKey, Identity]`**: Marks `Id` as the primary key with AUTO_INCREMENT behavior.
- **`[Column]`**: Maps each property to a database column.
- **`[NotNull]`**: Indicates that the column does not allow NULL values.
- **`?` Syntax**: Indicates nullable properties (can be empty).
- **`decimal`**: Used for monetary amounts to avoid floating-point precision issues.

The data model has been successfully created.

---

### Step 5: Create the AppDataConnection Class

The `AppDataConnection` class serves as the database connection manager using LINQ2DB. This class handles all direct MySQL database communication.

**Instructions:**

1. In the **Solution Explorer**, right-click on the **Models** folder.
2. Select **Add → New Item**.
3. Choose **Class** and name it **AppDataConnection.cs**.
4. Replace the default code with the following:

```csharp
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.MySql;
using System.Configuration;

namespace MySQLMVC5Grid.Models
{
    /// <summary>
    /// Database connection manager for MySQL using LINQ2DB.
    /// Handles all MySQL database operations and connection configuration.
    /// Uses MySqlConnector (modern MySQL connector) instead of legacy MySql.Data.
    /// </summary>
    public sealed class AppDataConnection : DataConnection
    {
        /// <summary>
        /// Initializes a new instance of AppDataConnection with MySQL configuration.
        /// Retrieves the connection string from web.config and configures for MySQL 8.0+ with MySqlConnector.
        /// </summary>
        public AppDataConnection() :
            base(new DataOptions()
                .UseMySql(
                    ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString,
                    MySqlVersion.MySql80,
                    MySqlProvider.MySqlConnector))
        {
            // Enable inline parameters for better SQL logging in debug mode.
            // This allows you to see the actual SQL query with parameters in the debugger.
            this.InlineParameters = true;

            // Optional: Enable query tracing for debugging purposes.
            // Uncomment the following line to enable SQL query logging to the Debug output:
            // this.OnTrace = info => System.Diagnostics.Debug.WriteLine(info.SqlText);
        }

        /// <summary>
        /// Provides access to the Transactions table for LINQ2DB queries.
        /// </summary>
        public ITable<Transactions> Transactions => this.GetTable<Transactions>();
    }
}
```

**Explanation:**

- **`AppDataConnection` Class**: Inherits from `LinqToDB.Data.DataConnection`, which manages the MySQL connection lifecycle.
- **`UseMySql(...)`**: Configures LINQ2DB for MySQL with version 8.0 and MySqlConnector provider.
- **`MySqlVersion.MySql80`**: Specifies MySQL 8.0+ compatibility. Use `MySql57` for MySQL 5.7.
- **`MySqlProvider.MySqlConnector`**: Uses the modern MySqlConnector library instead of the legacy MySql.Data.
- **`InlineParameters`**: When set to `true`, enables inline parameter logging, making SQL debugging easier.
- **`Transactions` Property**: Returns an `ITable<Transactions>` interface that allows LINQ queries against the transactions table.
- **Connection String**: Retrieved from the `MySqlConn` entry in `web.config` (configured in Step 6).

The AppDataConnection class is now ready for use in controllers.

---

### Step 6: Configure the Connection String in web.config

A connection string contains the information needed to connect the application to the MySQL database.

**Instructions:**

1. Open the `Web.config` file located in the project root.
2. Find the `<connectionStrings>` section (or create one if it doesn't exist).
3. Add or replace the connection string with the following:

```xml
<connectionStrings>
    <add name="MySqlConn" 
         connectionString="Server=localhost;Port=3306;Database=transactiondb;User Id=root;Password=YourPassword;" 
         providerName="MySqlConnector" />
</connectionStrings>
```

**Connection String Components:**

| Component | Description | Example |
|-----------|-------------|---------|
| **Server** | The address of the MySQL server (hostname, IP address, or localhost) | `localhost` or `192.168.1.100` or `mysql.example.com` |
| **Port** | MySQL server port (default is 3306) | `3306` |
| **Database** | The database name | `transactiondb` |
| **User Id** | MySQL username | `root` or `appuser` |
| **Password** | MySQL password | `YourPassword` |
| **providerName** | The MySQL connector provider | `MySqlConnector` (modern) or `MySql.Data` (legacy) |
| **Charset** | Character set encoding (optional) | `utf8mb4` |
| **SslMode** | SSL/TLS encryption mode (optional) | `None`, `Required`, `Preferred` |

**Common Connection String Scenarios:**

- **Local Development (Root User):**
  ```xml
  Server=localhost;Port=3306;Database=transactiondb;User Id=root;Password=YourPassword;
  ```

- **Remote MySQL Server (SQL Authentication):**
  ```xml
  Server=mysql.example.com;Port=3306;Database=transactiondb;User Id=appuser;Password=SecurePassword;SslMode=Preferred;
  ```

- **Local Development with Specific Charset:**
  ```xml
  Server=localhost;Port=3306;Database=transactiondb;User Id=root;Password=YourPassword;Charset=utf8mb4;
  ```

- **MySQL Server on Non-Standard Port:**
  ```xml
  Server=192.168.1.100;Port=3307;Database=transactiondb;User Id=root;Password=YourPassword;
  ```

- **Production with SSL Encryption:**
  ```xml
  Server=mysql.production.com;Port=3306;Database=transactiondb;User Id=produser;Password=SecurePassword;SslMode=Required;AllowPublicKeyRetrieval=False;
  ```

**MySQL User and Permission Setup:**

If you need to create a dedicated MySQL user instead of using root:

```sql
-- In MySQL command line as root user:
CREATE USER 'appuser'@'localhost' IDENTIFIED BY 'SecurePassword';
GRANT ALL PRIVILEGES ON transactiondb.* TO 'appuser'@'localhost';
FLUSH PRIVILEGES;
```

For remote connections:

```sql
CREATE USER 'appuser'@'%' IDENTIFIED BY 'SecurePassword';
GRANT ALL PRIVILEGES ON transactiondb.* TO 'appuser'@'%';
FLUSH PRIVILEGES;
```

The connection string has been successfully configured.

---

## Implementing the Grid with UrlAdaptor

The **UrlAdaptor** approach delegates all CRUD operations and data processing (filtering, sorting, paging) to the server-side controller. This is the simpler and most common approach for standard data operations.

### Step 7: Create the GridController for UrlAdaptor

The `GridController` handles all HTTP requests from the Syncfusion Grid, including data retrieval, create, update, delete, and batch operations for MySQL.

**Instructions:**

1. In the **Solution Explorer**, right-click on the **Controllers** folder.
2. Select **Add → Controller**.
3. Choose **MVC 5 Controller - Empty**.
4. Name it **GridController.cs**.
5. Replace the default code with the following:

```csharp
using LinqToDB;
using LinqToDB.Async;
using MySQLMVC5Grid.Models;
using Syncfusion.EJ2.Base;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MySQLMVC5Grid.Controllers
{
    /// <summary>
    /// Handles all Syncfusion EJ2 Grid data operations (CRUD and data processing) for MySQL.
    /// Uses LINQ2DB for direct MySQL database access and Syncfusion DataManager for data operations.
    /// </summary>
    public class GridController : Controller
    {
        /// <summary>
        /// Returns the main Grid view.
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Processes the DataManager request to perform searching, filtering, sorting, and paging operations.
        /// This method is called by the UrlAdaptor for all data retrieval operations from MySQL.
        /// </summary>
        /// <param name="DataManagerRequest">Contains the details of the data operation requested (filters, sorts, paging, search).</param>
        /// <returns>Returns a JSON object with the filtered, sorted, and paginated data along with the total record count.</returns>
        [HttpPost]
        public async Task<ActionResult> UrlDatasource(DataManagerRequest DataManagerRequest)
        {
            try
            {
                using (var db = new AppDataConnection())
                {
                    // Retrieve data from the MySQL database using LINQ2DB.
                    // The FromSql method executes raw SQL and returns an IQueryable<Transactions>.
                    // This IQueryable allows Syncfusion's built-in data operation methods to manipulate the data server-side.
                    IQueryable<Transactions> query = db.FromSql<Transactions>(
                        @"SELECT * FROM transactiondb.transactions");
                    
                    // Initialize the QueryableOperation instance for handling Syncfusion data operations.
                    QueryableOperation operation = new QueryableOperation();

                    // === SEARCHING ===
                    // If search filters are provided, apply them to the query.
                    if (DataManagerRequest.Search != null && DataManagerRequest.Search.Count > 0)
                    {
                        query = operation.PerformSearching(query, DataManagerRequest.Search);
                    }

                    // === FILTERING ===
                    // If where clauses are provided, filter the data accordingly.
                    if (DataManagerRequest.Where != null && DataManagerRequest.Where.Count > 0)
                    {
                        query = operation.PerformFiltering(query, DataManagerRequest.Where, DataManagerRequest.Where[0].Operator);
                    }

                    // === SORTING ===
                    // If sorting is requested, apply the sort operations.
                    if (DataManagerRequest.Sorted != null && DataManagerRequest.Sorted.Count > 0)
                    {
                        query = operation.PerformSorting(query, DataManagerRequest.Sorted);
                    }

                    // === TOTAL COUNT ===
                    // Get the total number of records after filtering but before paging.
                    // This is used by the Grid to display pagination information.
                    int count = await query.CountAsync();

                    // === PAGING (SKIP) ===
                    // Skip the specified number of records to implement pagination.
                    if (DataManagerRequest.Skip != 0)
                    {
                        query = operation.PerformSkip(query, DataManagerRequest.Skip);
                    }

                    // === PAGING (TAKE) ===
                    // Take only the specified number of records for the current page.
                    if (DataManagerRequest.Take != 0)
                    {
                        query = operation.PerformTake(query, DataManagerRequest.Take);
                    }

                    // Execute the query and retrieve the final dataset from MySQL.
                    var rows = await query.ToListAsync();

                    // Return the data and total count to the client.
                    // The Grid uses this response to populate and display the records.
                    // RequiresCounts determines whether to include the total record count.
                    return DataManagerRequest.RequiresCounts ? Json(new { result = rows, count }) : Json(rows);
                }
            }
            catch (Exception ex)
            {
                // Log the exception and return an error response.
                System.Diagnostics.Debug.WriteLine($"UrlDatasource Error: {ex.Message}");
                return Json(new { IsError = true, ErrorMessage = ex.Message });
            }
        }

        // ===== CREATE OPERATION =====
        /// <summary>
        /// Handles the insertion of a new transaction record into MySQL.
        /// Called when the user adds a new row to the Grid and submits the form.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Insert(Transactions value)
        {
            try
            {
                using (var db = new AppDataConnection())
                {
                    // Insert the new transaction and retrieve the generated identity value.
                    // LINQ2DB automatically handles the MySQL-generated Id via AUTO_INCREMENT.
                    var newId = await db.InsertWithInt32IdentityAsync(value);
                    value.Id = newId;

                    // Return the created record with its generated ID.
                    // The Grid expects the complete row data in the response.
                    return Json(value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Insert Error: {ex.Message}");
                return Json(new { IsError = true, ErrorMessage = ex.Message });
            }
        }

        // ===== UPDATE OPERATION =====
        /// <summary>
        /// Handles the update of an existing transaction record in MySQL.
        /// Called when the user edits a row and submits the changes.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Update(Transactions value)
        {
            try
            {
                using (var db = new AppDataConnection())
                {
                    // Verify the record exists before attempting to update.
                    bool exists = await db.Transactions.AnyAsync(t => t.Id == value.Id);
                    if (!exists)
                    {
                        return Json(new { IsError = true, ErrorMessage = "Record not found" });
                    }

                    // Update the record in the MySQL database.
                    // LINQ2DB updates only the columns that are mapped in the entity.
                    var rows = await db.UpdateAsync(value);

                    // Return the updated record.
                    return Json(value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update Error: {ex.Message}");
                return Json(new { IsError = true, ErrorMessage = ex.Message });
            }
        }

        // ===== DELETE OPERATION =====
        /// <summary>
        /// Handles the deletion of a transaction record from MySQL.
        /// Called when the user deletes a row from the Grid.
        /// The primary key is passed via the CRUDModel parameter.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Remove(CRUDModel<Transactions> model)
        {
            try
            {
                var key = Convert.ToInt32(model.Key);
                using (var db = new AppDataConnection())
                {
                    // Delete the record by its primary key.
                    var rows = await db.Transactions.DeleteAsync(t => t.Id == key);

                    // Verify that a record was actually deleted.
                    if (rows == 0)
                    {
                        return Json(new { IsError = true, ErrorMessage = "Record not found" });
                    }

                    // Return the model to confirm the deletion.
                    return Json(model);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete Error: {ex.Message}");
                return Json(new { IsError = true, ErrorMessage = ex.Message });
            }
        }

        // ===== BATCH UPDATE OPERATION =====
        /// <summary>
        /// Handles batch operations (multiple inserts, updates, and deletes in a single request).
        /// The Grid sends all changes at once for efficiency.
        /// All operations are wrapped in a MySQL transaction to ensure data consistency.
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> BatchUpdate(CRUDModel<Transactions> payload)
        {
            try
            {
                using (var db = new AppDataConnection())
                {
                    // Begin a MySQL database transaction to ensure atomicity.
                    // If any operation fails, all changes are rolled back.
                    var tr = await db.BeginTransactionAsync();

                    try
                    {
                        // === BATCH INSERT ===
                        if (payload.Added != null && payload.Added.Count > 0)
                        {
                            foreach (var r in payload.Added)
                            {
                                var newId = await db.InsertWithInt32IdentityAsync(r);
                                r.Id = newId;
                            }
                        }

                        // === BATCH UPDATE ===
                        if (payload.Changed != null && payload.Changed.Count > 0)
                        {
                            foreach (var r in payload.Changed)
                            {
                                // Fine-grained update: explicitly set each column.
                                await db.Transactions
                                    .Where(t => t.Id == r.Id)
                                    .Set(t => t.TransactionId, r.TransactionId)
                                    .Set(t => t.CustomerId, r.CustomerId)
                                    .Set(t => t.OrderId, r.OrderId)
                                    .Set(t => t.InvoiceNumber, r.InvoiceNumber)
                                    .Set(t => t.Description, r.Description)
                                    .Set(t => t.Amount, r.Amount)
                                    .Set(t => t.CurrencyCode, r.CurrencyCode)
                                    .Set(t => t.TransactionType, r.TransactionType)
                                    .Set(t => t.PaymentGateway, r.PaymentGateway)
                                    .Set(t => t.CreatedAt, r.CreatedAt)
                                    .Set(t => t.CompletedAt, r.CompletedAt)
                                    .Set(t => t.Status, r.Status)
                                    .UpdateAsync();
                            }
                        }

                        // === BATCH DELETE ===
                        if (payload.Deleted != null && payload.Deleted.Count > 0)
                        {
                            var keys = payload.Deleted.Select(d => d.Id).ToArray();
                            await db.Transactions.Where(t => keys.Contains(t.Id)).DeleteAsync();
                        }

                        // Commit the transaction after all operations succeed.
                        await tr.CommitAsync();

                        return Json(payload, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception innerEx)
                    {
                        // Rollback on any error.
                        await tr.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine($"Batch Update Error: {innerEx.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Batch Update Exception: {ex.Message}");
                return Json(new { IsError = true, ErrorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
```

**Understanding the UrlAdaptor Controller Flow for MySQL:**

1. **UrlDatasource Method**: Receives filtering, sorting, and paging requests from the Grid and returns processed data from MySQL.
2. **QueryableOperation**: Syncfusion's built-in class that applies filters, sorts, searches, and pagination to an `IQueryable<T>` collection.
3. **CRUD Operations**: Each HTTP POST method (Insert, Update, Remove) handles a specific data operation with MySQL.
4. **Batch Operations**: The BatchUpdate method optimizes performance by handling multiple changes in a single MySQL transaction.
5. **MySqlConnector**: All operations use the modern MySqlConnector library for robust MySQL connectivity.

[**SCREENSHOT PLACEHOLDER**: Add screenshot of UrlDatasource method showing the SQL query formation in controller when filtering/sorting is applied on MySQL data. This helps understand how Syncfusion DataManager constructs MySQL queries with proper backtick escaping for reserved words. You can capture this from Visual Studio debugger by setting a breakpoint on the `query` variable after operations are applied.]

The GridController for UrlAdaptor is now complete.

---

### Step 8: Create the Grid View for UrlAdaptor

The Grid View displays the Syncfusion EJ2 Grid component on the client-side, configured to use the UrlAdaptor pattern.

**Instructions:**

1. In the **Solution Explorer**, right-click on **Views → Grid** (create the Grid folder if it doesn't exist).
2. Right-click and select **Add → New Item → Razor View**.
3. Name it **Index.cshtml**.
4. Replace the default code with the following:

```html
@{
    ViewBag.Title = "Syncfusion EJ2 Grid - MySQL UrlAdaptor";
}

<h2>Financial Transactions - UrlAdaptor</h2>

@(Html.EJ2().Grid<dynamic>()
    .ID("grid")
    .DataSource(dataSource => dataSource
        .Url(Url.Action("UrlDatasource", "Grid"))  // Server-side data source endpoint
        .Adaptor(AdaptorType.UrlAdaptor)           // Use UrlAdaptor for CRUD operations
        .CrossDomain(true)
    )
    .ActionComplete("onActionComplete")
    .AllowPaging()
    .AllowSorting()
    .AllowSearching()
    .AllowGrouping()
    .AllowFiltering(filter => filter.Type(FilterType.Menu))
    .Columns(col =>
    {
        col.Field("Id").HeaderText("ID").Width(80).IsPrimaryKey(true).Visible(false).Add();
        col.Field("TransactionId").HeaderText("Transaction ID").Width(120).Add();
        col.Field("CustomerId").HeaderText("Customer ID").Width(100).Add();
        col.Field("OrderId").HeaderText("Order ID").Width(100).Add();
        col.Field("InvoiceNumber").HeaderText("Invoice").Width(120).Add();
        col.Field("Description").HeaderText("Description").Width(200).Add();
        col.Field("Amount").HeaderText("Amount").Width(100).Format("C2").TextAlign(TextAlign.Right).Add();
        col.Field("CurrencyCode").HeaderText("Currency").Width(100).Add();
        col.Field("TransactionType").HeaderText("Type").Width(100).Add();
        col.Field("PaymentGateway").HeaderText("Gateway").Width(120).Add();
        col.Field("Status").HeaderText("Status").Width(100).Add();
        col.Field("CreatedAt").HeaderText("Created").Width(150).Type("date").Format("yMd hh:mm").Add();
    })
    .EditSettings(edit => edit.AllowEditing(true).AllowAdding(true).AllowDeleting(true).Mode(EditMode.Dialog))
    .ToolbarClick("toolbarClick")
    .Toolbar(new List<string> { "Add", "Edit", "Delete", "Update", "Cancel", "Search" })
    .PageSettings(page => page.PageSize(10))
)

<script>
    function onActionComplete(args) {
        console.log("Action Completed: ", args);
    }

    function toolbarClick(args) {
        var gridObj = document.getElementById("grid").ej2_instances[0];
        if (args.item.id === "grid_add") {
            // Add action
        } else if (args.item.id === "grid_edit") {
            // Edit action
        } else if (args.item.id === "grid_delete") {
            // Delete action
        }
    }
</script>
```

**Explanation:**

- **`Url(Url.Action("UrlDatasource", "Grid"))`**: Points to the UrlDatasource method in the GridController.
- **`Adaptor(AdaptorType.UrlAdaptor)`**: Tells the Grid to use the UrlAdaptor pattern for CRUD operations.
- **`AllowPaging()`, `AllowSorting()`, `AllowFiltering()`**: Enables these features, with server-side processing through UrlAdaptor.
- **`Format("C2")`**: Formats the Amount column as currency with 2 decimal places.
- **`EditSettings(...Mode(EditMode.Dialog))`**: Enables inline editing with a dialog form.
- **Toolbar**: Provides buttons for Add, Edit, Delete, Update, Cancel, and Search operations.

The UrlAdaptor Grid view is now complete and ready for testing.

---

## Implementing the Grid with CustomAdaptor

The **CustomAdaptor** approach provides more control over data processing. While it still uses server-side processing, it allows for custom business logic in the controller.

### Step 9: Create GridController for CustomAdaptor

For the CustomAdaptor pattern, create a new controller or extend the existing GridController with custom methods.

**Instructions:**

1. Add the following method to the existing **GridController.cs**:

```csharp
/// <summary>
/// Processes the DataManager request using CustomAdaptor pattern for MySQL.
/// This method is identical to UrlDatasource but demonstrates the CustomAdaptor approach.
/// Use this when you need to implement custom business logic alongside standard data operations.
/// </summary>
[HttpPost]
public async Task<ActionResult> CustomDatasource(DataManagerRequest DataManagerRequest)
{
    try
    {
        using (var db = new AppDataConnection())
        {
            // === CUSTOM BUSINESS LOGIC ===
            // Example: Filter transactions based on custom criteria.
            // You can add role-based filtering, status-based filtering, etc.
            
            // Example 1: Filter only successful transactions
            // var query = db.Transactions.Where(t => t.Status == "SUCCESS");
            
            // Example 2: Filter by date range
            // var startDate = new DateTime(2026, 01, 01);
            // var endDate = new DateTime(2026, 01, 31);
            // var query = db.Transactions.Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate);

            // Retrieve all transactions from MySQL database.
            IQueryable<Transactions> query = db.FromSql<Transactions>(
                @"SELECT * FROM transactiondb.transactions");
            
            QueryableOperation operation = new QueryableOperation();

            // Apply standard Syncfusion data operations.
            if (DataManagerRequest.Search != null && DataManagerRequest.Search.Count > 0)
            {
                query = operation.PerformSearching(query, DataManagerRequest.Search);
            }

            if (DataManagerRequest.Where != null && DataManagerRequest.Where.Count > 0)
            {
                query = operation.PerformFiltering(query, DataManagerRequest.Where, DataManagerRequest.Where[0].Operator);
            }

            if (DataManagerRequest.Sorted != null && DataManagerRequest.Sorted.Count > 0)
            {
                query = operation.PerformSorting(query, DataManagerRequest.Sorted);
            }

            int count = await query.CountAsync();

            if (DataManagerRequest.Skip != 0)
            {
                query = operation.PerformSkip(query, DataManagerRequest.Skip);
            }

            if (DataManagerRequest.Take != 0)
            {
                query = operation.PerformTake(query, DataManagerRequest.Take);
            }

            var rows = await query.ToListAsync();

            return DataManagerRequest.RequiresCounts ? Json(new { result = rows, count }) : Json(rows);
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"CustomDatasource Error: {ex.Message}");
        return Json(new { IsError = true, ErrorMessage = ex.Message });
    }
}
```

This CustomAdaptor method allows you to implement custom business logic (status-based filtering, date range queries, custom calculations, etc.) before and after applying Syncfusion's standard data operations.

---

### Step 10: Create the Grid View for CustomAdaptor

**Instructions:**

1. Create a new View file named **CustomIndex.cshtml** in the **Views/Grid** folder.
2. Replace the default code with the following:

```html
@{
    ViewBag.Title = "Syncfusion EJ2 Grid - MySQL CustomAdaptor";
}

<h2>Financial Transactions - CustomAdaptor</h2>

@(Html.EJ2().Grid<dynamic>()
    .ID("gridCustom")
    .DataSource(dataSource => dataSource
        .Url(Url.Action("CustomDatasource", "Grid"))  // Custom endpoint for MySQL
        .Adaptor(AdaptorType.UrlAdaptor)              // Use UrlAdaptor (CustomAdaptor uses custom POST methods)
        .CrossDomain(true)
    )
    .AllowPaging()
    .AllowSorting()
    .AllowSearching()
    .AllowFiltering(filter => filter.Type(FilterType.Menu))
    .Columns(col =>
    {
        col.Field("Id").HeaderText("ID").Width(80).IsPrimaryKey(true).Visible(false).Add();
        col.Field("TransactionId").HeaderText("Transaction ID").Width(120).Add();
        col.Field("CustomerId").HeaderText("Customer ID").Width(100).Add();
        col.Field("OrderId").HeaderText("Order ID").Width(100).Add();
        col.Field("InvoiceNumber").HeaderText("Invoice").Width(120).Add();
        col.Field("Description").HeaderText("Description").Width(200).Add();
        col.Field("Amount").HeaderText("Amount").Width(100).Format("C2").TextAlign(TextAlign.Right).Add();
        col.Field("Status").HeaderText("Status").Width(100).Add();
        col.Field("CreatedAt").HeaderText("Created").Width(150).Type("date").Format("yMd hh:mm").Add();
    })
    .EditSettings(edit => edit.AllowEditing(true).AllowAdding(true).AllowDeleting(true).Mode(EditMode.Dialog))
    .Toolbar(new List<string> { "Add", "Edit", "Delete", "Update", "Cancel", "Search" })
    .PageSettings(page => page.PageSize(10))
)
```

---

## Troubleshooting and Common Issues

### Issue 1: Microsoft.AspNet.Mvc Version Mismatch

**Problem:**
```
Could not load file or assembly 'Microsoft.AspNet.Mvc, Version=5.2.9.0' or one of its dependencies.
```

or when using Syncfusion components:
```
'Syncfusion.EJ2.MVC5' is not compatible with this version of ASP.NET MVC.
```

**Root Cause:**

When creating a new ASP.NET MVC5 project using the Visual Studio template, the default installed version of `Microsoft.AspNet.Mvc` is **5.2.9** refer [wikipedia](https://en.wikipedia.org/wiki/ASP.NET_MVC) for more details. However, Syncfusion packages (version 32.1.19) are built and compiled against **5.3.0**, causing compatibility issues.

**Solution:**

1. Open **Package Manager Console** in Visual Studio.
2. Run the following command:

```powershell
Update-Package Microsoft.AspNet.Mvc -Version 5.3.0
```

3. Wait for the update to complete.
4. Clean and rebuild the solution:
   - **Build → Clean Solution**
   - **Build → Build Solution**

5. Verify the version in `packages.config` now shows:
   ```xml
   <package id="Microsoft.AspNet.Mvc" version="5.3.0" targetFramework="net481" />
   ```

This ensures all Syncfusion components and dependencies use the correct framework version.

---

### Issue 2: MySQL Connection String Issues

**Problem:**
```
Unable to connect to any of the specified MySQL hosts.
```

or

```
Unknown database 'transactiondb'
```

or

```
Access denied for user 'root'@'localhost'
```

**Root Causes and Solutions:**

#### 2a: Incorrect Server Address or Port

**Diagnosis:**
- Check the `Server` and `Port` in your connection string.
- Verify MySQL is running on your server.

**Solutions:**

For **Local Development (Default Port 3306):**
```xml
Server=localhost;Port=3306;Database=transactiondb;User Id=root;Password=YourPassword;
```

For **Different Port (Non-Standard):**
```xml
Server=localhost;Port=3307;Database=transactiondb;User Id=root;Password=YourPassword;
```

For **Remote MySQL Server:**
```xml
Server=mysql.example.com;Port=3306;Database=transactiondb;User Id=appuser;Password=SecurePassword;
```

For **MySQL Running in Docker:**
```xml
Server=host.docker.internal;Port=3306;Database=transactiondb;User Id=root;Password=YourPassword;
```

#### 2b: Database Does Not Exist

**Diagnosis:**
- Verify that the database `transactiondb` has been created in MySQL.

**Solution:**
```sql
-- In MySQL command line, execute:
CREATE DATABASE IF NOT EXISTS transactiondb CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE transactiondb;
```

#### 2c: Incorrect Username or Password

**Diagnosis:**
- You're using incorrect MySQL credentials.

**Solution:**

Test the connection using MySQL command line:
```bash
mysql -h localhost -u root -p
```

If the credentials are wrong, reset the MySQL root password (varies by OS):

**For Windows:**
```cmd
net stop MySQL80
mysqld --skip-grant-tables
-- In another terminal:
mysql -u root
FLUSH PRIVILEGES;
ALTER USER 'root'@'localhost' IDENTIFIED BY 'NewPassword';
EXIT;
```

**For Linux:**
```bash
sudo mysql -u root
ALTER USER 'root'@'localhost' IDENTIFIED BY 'NewPassword';
FLUSH PRIVILEGES;
EXIT;
```

#### 2d: MySQL User Does Not Have Sufficient Permissions

**Diagnosis:**
- The user exists but doesn't have the required permissions on the database.

**Solution:**

Grant permissions to the user:
```sql
-- As root user:
GRANT ALL PRIVILEGES ON transactiondb.* TO 'appuser'@'localhost';
FLUSH PRIVILEGES;
```

For remote connections:
```sql
GRANT ALL PRIVILEGES ON transactiondb.* TO 'appuser'@'%';
FLUSH PRIVILEGES;
```

#### 2e: SSL/TLS Connection Issues

**Problem:**
```
SSL Connection error: unable to get local issuer certificate
```

**Solutions:**

Option 1: Disable SSL (development only):
```xml
Server=localhost;Port=3306;Database=transactiondb;User Id=root;Password=YourPassword;SslMode=None;
```

Option 2: Allow public key retrieval:
```xml
Server=localhost;Port=3306;Database=transactiondb;User Id=root;Password=YourPassword;AllowPublicKeyRetrieval=True;
```

Option 3: Prefer SSL but don't require:
```xml
Server=localhost;Port=3306;Database=transactiondb;User Id=root;Password=YourPassword;SslMode=Preferred;
```

---

### Issue 3: MySqlConnector vs MySql.Data - Which One to Use?

**Problem:**

Should I use MySqlConnector or the older MySql.Data package?

**Solution:**

| Aspect | MySqlConnector | MySql.Data |
|--------|---|---|
| **Status** | Modern, actively maintained | Legacy, deprecated |
| **Performance** | Faster, optimized | Slower |
| **MySQL 8.0+ Support** | Full support | Limited support |
| **Async Support** | Full async/await | Partial async |
| **License** | MIT (open source) | GPL (complicated) |
| **Recommendation** | **Use this** | Avoid for new projects |

**Migration from MySql.Data to MySqlConnector:**

1. In your connection string, use:
   ```xml
   providerName="MySqlConnector"
   ```

2. Ensure your LINQ2DB configuration uses:
   ```csharp
   .UseMySql(..., MySqlVersion.MySql80, MySqlProvider.MySqlConnector)
   ```

3. Remove the old MySql.Data package:
   ```powershell
   Uninstall-Package MySql.Data
   ```

---

### Issue 4: LINQ2DB MySQL Mapping Errors

**Problem:**
```
No columns matched for entity mapping
```

or

```
MySqlException: Unknown column 'Transactions.TransactionId'
```

**Root Cause:**

The `[Table]` or `[Column]` attributes in the `Transactions` model class don't match the actual MySQL table schema.

**Solution:**

1. Verify the exact table and column names in MySQL:
   ```sql
   DESCRIBE transactiondb.transactions;
   ```

2. Update the `Transactions.cs` model to match exactly:
   ```csharp
   [Table("transactions")]  // MySQL table names are case-sensitive on Linux
   public class Transactions
   {
       [Column]
       public string TransactionId { get; set; }  // Column names must match database exactly
   }
   ```

3. Ensure all properties are decorated with `[Column]`.

4. MySQL is case-sensitive on Linux but case-insensitive on Windows/Mac. Always use lowercase table names for portability.

---

### Issue 5: Character Encoding Issues with UTF-8

**Problem:**
```
Incorrect string value: '\xF0\x9F\x98\x80...' for column 'Description'
```

or special characters (emoji, accented letters) are saved as question marks.

**Root Cause:**

The MySQL table or column is not using the `utf8mb4` character set, which is required for full UTF-8 support including emojis.

**Solution:**

1. **Alter existing table:**
   ```sql
   ALTER TABLE transactions CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
   ALTER TABLE transactions MODIFY Description VARCHAR(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
   ```

2. **When creating new tables, always use utf8mb4:**
   ```sql
   CREATE TABLE transactions (
       Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
       Description VARCHAR(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci,
       ...
   ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
   ```

3. **In your connection string, add Charset:**
   ```xml
   Server=localhost;Port=3306;Database=transactiondb;User Id=root;Password=YourPassword;Charset=utf8mb4;
   ```

---

### Issue 6: LINQ2DB vs Entity Framework - When to Use Each with MySQL

**Problem:**

When should I use LINQ2DB vs Entity Framework (EF) for MySQL?

**Solution:**

| Aspect | LINQ2DB | Entity Framework Core |
|--------|---------|-----|
| **Learning Curve** | Lower | Higher |
| **Performance with MySQL** | Very fast | Good |
| **MySQL-Specific Features** | Good support | Good support |
| **Configuration** | Simple (connection string) | Complex (DbContext setup) |
| **Database Migrations** | Manual SQL | Built-in with EF Migrations |
| **Use Case** | Simple CRUD, web grids, lightweight apps | Complex business logic, enterprise apps |
| **Syncfusion Grid Compatibility** | Excellent | Good (use EF-specific docs) |

**For this guide, LINQ2DB is recommended because:**

1. It's lightweight and performs well with MySQL.
2. Syncfusion's QueryableOperation integrates seamlessly with LINQ2DB's `IQueryable<T>`.
3. It requires minimal configuration compared to Entity Framework.
4. It's sufficient for standard CRUD and data grid operations.
5. Syncfusion provides separate documentation for Entity Framework Core with Blazor.

---

### Issue 7: Grid Not Loading Data

**Problem:**

The Grid appears empty or shows no data even though the MySQL database has records.

**Diagnosis Steps:**

1. **Enable Console Logging:** Open browser DevTools (F12) and check the Console tab for JavaScript errors.

2. **Check Network Requests:** In DevTools, go to **Network** tab and look for the request to `UrlDatasource`. Verify:
   - Status code is 200 (success).
   - Response contains JSON data.

3. **Check Server Logs:** In Visual Studio, check the **Output** window for any exceptions.

4. **Test MySQL Connection:** In Package Manager Console, verify the connection string works:
   ```powershell
   # Test by creating a simple console app or using MySQL Workbench
   ```

**Common Solutions:**

1. **Verify the controller endpoint URL:**
   ```csharp
   .Url(Url.Action("UrlDatasource", "Grid"))  // Correct controller and method name
   ```

2. **Ensure the Grid ID matches:**
   ```html
   @(Html.EJ2().Grid<dynamic>().ID("grid") ...)  <!-- ID must match JavaScript references -->
   ```

3. **Enable debugging:** Add breakpoints in the controller's `UrlDatasource` method to verify it's being called with MySQL data.

4. **Check CORS if applicable:** If the data endpoint is on a different domain:
   ```csharp
   .CrossDomain(true)  // Enable cross-domain requests
   ```

5. **Verify MySQL query is correct:** Check that the SQL query in UrlDatasource is accessing the right database and table:
   ```csharp
   IQueryable<Transactions> query = db.FromSql<Transactions>(
       @"SELECT * FROM transactiondb.transactions");  // Ensure database.table format
   ```

---

### Issue 8: Decimal Precision Issues with Currency

**Problem:**
```
Amount stored as 153399.00 is retrieved as 153399 (no decimal places).
```

or

```
Rounding errors when performing calculations on decimal amounts.
```

**Root Cause:**

The Amount column is mapped as `decimal` but MySQL or the display is not preserving decimal places.

**Solution:**

1. **In the Transactions model, ensure decimal mapping:**
   ```csharp
   [Column, NotNull]
   public decimal Amount { get; set; }  // Explicitly use decimal
   ```

2. **In MySQL table, ensure proper precision:**
   ```sql
   ALTER TABLE transactions MODIFY Amount DECIMAL(15,2);
   -- 15 total digits, 2 decimal places: max 9,999,999,999,999.99
   ```

3. **In the Grid view, format for display:**
   ```html
   col.Field("Amount")
       .HeaderText("Amount")
       .Format("C2")  // Currency with 2 decimal places
       .TextAlign(TextAlign.Right)
       .Add();
   ```

4. **When updating, ensure decimal values are submitted correctly:**
   ```csharp
   // In JavaScript (if needed):
   var amount = parseFloat(document.getElementById("amount").value);
   ```

---

### Issue 9: Timestamp/DateTime Issues

**Problem:**
```
DateTime value '2026-01-13 10:15:30' is showing as '2026-01-13T10:15:30.000' or in wrong timezone.
```

or

```
CreatedAt field always shows current time even when reading from database.
```

**Root Cause:**

MySQL DATETIME differs from .NET DateTime in timezone handling and format.

**Solution:**

1. **In MySQL, use DATETIME with DEFAULT:**
   ```sql
   CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
   CompletedAt DATETIME NULL
   ```

2. **In the model, use System.DateTime:**
   ```csharp
   [Column, NotNull]
   public DateTime CreatedAt { get; set; }  // Not DateTime?
   
   [Column]
   public DateTime? CompletedAt { get; set; }  // Nullable if optional
   ```

3. **Don't update timestamp columns on CRUD:**
   ```csharp
   // In Update method, don't include CreatedAt:
   await db.Transactions
       .Where(t => t.Id == r.Id)
       .Set(t => t.TransactionId, r.TransactionId)
       // ... other fields
       // Don't update: .Set(t => t.CreatedAt, r.CreatedAt)
       .UpdateAsync();
   ```

4. **In Grid view, format datetime properly:**
   ```html
   col.Field("CreatedAt")
       .HeaderText("Created At")
       .Type("date")
       .Format("yMd hh:mm")  // Date and time format
       .Add();
   ```

---

### Issue 10: Syncfusion License Warning

**Problem:**

When running the application, a license warning appears in the browser console or as a banner.

**Solution:**

Syncfusion components require a valid license. Options:

1. **Use a trial license:**
   - Visit [Syncfusion License Registration](https://www.syncfusion.com/account/downloads).
   - Register your product and obtain a key.

2. **Register the license in the application:**
   - In **Global.asax.cs**, add:
   ```csharp
   protected void Application_Start()
   {
       AreaRegistration.RegisterAllAreas();
       FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
       RouteConfig.RegisterRoutes(RouteTable.Routes);
       BundleConfig.RegisterBundles(BundleTable.Bundles);

       // Register Syncfusion license
       Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR-LICENSE-KEY");
   }
   ```

3. **For development/testing:**
   - Use Syncfusion's [Community License](https://www.syncfusion.com/community/license) (free for organizations with less than $1 million in annual revenue).

---

## Summary

You have successfully created an ASP.NET MVC5 application with MySQL that includes:

✅ MySQL database setup with the `transactions` table with proper UTF-8 support  
✅ LINQ2DB for lightweight, efficient MySQL database operations  
✅ MySqlConnector for modern, reliable MySQL connectivity  
✅ Syncfusion EJ2 Grid with **UrlAdaptor** for standard CRUD operations  
✅ Syncfusion EJ2 Grid with **CustomAdaptor** for custom business logic  
✅ Full support for filtering, sorting, searching, paging, and batch operations  
✅ Proper decimal handling for currency amounts  
✅ UTF-8 character encoding for international and special characters  
✅ Error handling and troubleshooting guidelines specific to MySQL  

### Next Steps

1. **Test the application** by running it in Visual Studio (F5).
2. **Verify CRUD operations** by adding, editing, and deleting transaction records.
3. **Test with international characters** to ensure UTF-8 encoding works properly.
4. **Customize the Grid columns** based on your specific business requirements.
5. **Implement custom filtering** in the CustomAdaptor for advanced scenarios.
6. **Deploy to production** following your organization's deployment procedures.

### Additional Resources

- [Syncfusion EJ2 ASP.NET MVC Grid Documentation](https://www.syncfusion.com/aspnet-mvc-components/mvc-grid)
- [LINQ2DB Official Documentation](https://linq2db.github.io/)
- [MySqlConnector GitHub Documentation](https://github.com/mysql-net/MySqlConnector)
- [MySQL Official Documentation](https://dev.mysql.com/doc/)
- [ASP.NET MVC 5 Official Documentation](https://docs.microsoft.com/en-us/aspnet/mvc/mvc5)

---

**Documentation Version:** 1.0  
**Last Updated:** February 1, 2026  
**Compatible with:** Syncfusion 32.1.19, ASP.NET MVC 5.3.0, LINQ2DB 6.1.0, MySqlConnector 2.5.0, MySQL 5.7+, .NET Framework 4.8.1
