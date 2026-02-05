---
layout: post
title: ASP.NET MVC5 Data Grid connected to SQL via LINQ2DB | Syncfusion
description: Bind SQL Server data to ASP.NET MVC5 Data Grid using LINQ2DB with complete CRUD, filtering, sorting, paging, and advanced data operations using UrlAdaptor and CustomAdaptor patterns.
platform: ASP.NET MVC5
control: DataGrid (EJ2 Grid)
documentation: ug
---

# Connecting SQL Server to ASP.NET MVC5 Data Grid Using LINQ2DB

The [Syncfusion<sup style="font-size:70%">&reg;</sup> EJ2 ASP.NET MVC5 DataGrid](https://www.syncfusion.com/aspnet-mvc-components/mvc-grid) supports binding data from a SQL Server database. This documentation demonstrates how to integrate SQL Server with Syncfusion EJ2 Grid using LINQ2DB (Light-weight ORM) for data operations with both **UrlAdaptor** and **CustomAdaptor** approaches.

**What is LINQ2DB?**

LINQ2DB is a lightweight Object-Relational Mapping (ORM) library for .NET that simplifies database operations. It provides a bridge between C# code and databases like SQL Server, enabling type-safe queries without the overhead of heavier frameworks.

**Key Benefits of LINQ2DB for Syncfusion Grid Integration**

- **Lightweight Performance**: Minimal overhead compared to Entity Framework, ideal for web applications.
- **LINQ Support**: Use familiar LINQ syntax for database queries instead of raw SQL strings.
- **Type Safety**: Strong typing reduces runtime errors and provides IntelliSense support.
- **Built-in Security**: Automatic parameterization prevents SQL injection attacks.
- **Minimal Configuration**: Simple setup with straightforward connection string management.
- **Compatibility with Syncfusion DataManager**: Works seamlessly with Syncfusion EJ2 Grid's built-in data operations (filtering, sorting, paging, searching).

**What is the Difference Between UrlAdaptor and CustomAdaptor?**

| Aspect | UrlAdaptor | CustomAdaptor |
|--------|-----------|---------------|
| **Data Processing** | Server-side | Custom server-side logic |
| **HTTP Method** | POST requests to controller actions | Custom POST endpoints for batch operations |
| **Use Case** | Standard CRUD operations | Complex business logic, batch processing |
| **Built-in DataManager** | Relies on Syncfusion's QueryableOperation for filtering/sorting/paging | Manual implementation of query operations |
| **Configuration** | Simple URL mapping | Custom ActionResult methods |

---

## Prerequisites

Ensure the following software and packages are installed before proceeding:

| Software/Package | Version | Purpose |
|-----------------|---------|---------|
| Visual Studio 2022 | 17.0 or later | Development IDE with ASP.NET MVC workload |
| .NET Framework | 4.8.1 or compatible | Runtime framework |
| SQL Server | 2019 or later | Database server |
| Syncfusion.EJ2.MVC5 | 32.1.19 or later | EJ2 DataGrid and UI components for ASP.NET MVC5 |
| Syncfusion.EJ2.JavaScript | 32.1.19 or later | Client-side JavaScript for Syncfusion controls |
| Syncfusion.Licensing | 32.1.19 or later | Licensing for Syncfusion components |
| linq2db | 6.1.0 or later | Light-weight ORM for database operations |
| linq2db.SqlServer | 6.1.0 or later | SQL Server provider for LINQ2DB |
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

## Setting Up the SQL Server Environment

### Step 1: Create the Database and Table in SQL Server

First, the **SQL Server database** structure must be created to store ticket records.

**Instructions:**
1. Open SQL Server Management Studio (SSMS) or any SQL Server client.
2. Create a new database named `NetworkSupportDB`.
3. Define a `Tickets` table with the specified schema.
4. Insert sample data for testing.

Run the following SQL script:

```sql
-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'NetworkSupportDB')
BEGIN
    CREATE DATABASE NetworkSupportDB;
END
GO

USE NetworkSupportDB;
GO

-- Create Tickets Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tickets')
BEGIN
    CREATE TABLE dbo.Tickets (
        TicketId INT PRIMARY KEY IDENTITY(1,1),
        PublicTicketId VARCHAR(50) NOT NULL UNIQUE,
        Title VARCHAR(200) NULL,
        Description TEXT NULL,
        Category VARCHAR(100) NULL,
        Department VARCHAR(100) NULL,
        Assignee VARCHAR(100) NULL,
        CreatedBy VARCHAR(100) NULL,
        Status VARCHAR(50) NOT NULL DEFAULT 'Open',
        Priority VARCHAR(50) NOT NULL DEFAULT 'Medium',
        ResponseDue DATETIME2 NULL,
        DueDate DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
GO

-- Insert Sample Data (Optional)
INSERT INTO dbo.Tickets (PublicTicketId, Title, Description, Category, Department, Assignee, CreatedBy, Status, Priority, ResponseDue, DueDate, CreatedAt, UpdatedAt)
VALUES
('NET-1001', 'Network Connectivity Issue', 'Users unable to connect to the VPN', 'Network Issue', 'Network Ops', 'John Doe', 'Alice Smith', 'Open', 'High', '2026-01-14 10:00:00', '2026-01-15 17:00:00', '2026-01-13 10:15:30', '2026-01-13 10:15:30'),
('NET-1002', 'Server Performance Degradation', 'Email server responding slowly', 'Performance', 'Infrastructure', 'Emily White', 'Bob Johnson', 'InProgress', 'Critical', '2026-01-13 15:00:00', '2026-01-14 17:00:00', '2026-01-13 11:20:10', '2026-01-13 11:20:10');
GO
```

After executing this script, the ticket records are stored in the `Tickets` table within the `NetworkSupportDB` database. The database is now ready for integration with the ASP.NET MVC5 application.

---

### Step 2: Create a New ASP.NET MVC5 Project

Before installing NuGet packages, a new ASP.NET MVC5 Web Application must be created.

**Instructions:**

1. Open **Visual Studio 2022**.
2. Click **Create a new project**.
3. Search for **ASP.NET Web Application (.NET Framework)**.
4. Select the template and click **Next**.
5. Configure the project:
   - **Project name**: `MSSQLMVC5Grid` (or your preferred name)
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

NuGet packages are software libraries that add functionality to the application. These packages enable LINQ2DB, SQL Server connectivity, and Syncfusion Grid integration.

**Method 1: Using Package Manager Console (Recommended)**

1. Open Visual Studio 2022.
2. Navigate to **Tools → NuGet Package Manager → Package Manager Console**.
3. Run the following commands in sequence:

```powershell
Install-Package Syncfusion.EJ2.MVC5 -Version 32.1.19
Install-Package Syncfusion.EJ2.JavaScript -Version 32.1.19
Install-Package Syncfusion.Licensing -Version 32.1.19
Install-Package linq2db -Version 6.1.0
Install-Package linq2db.SqlServer -Version 6.1.0
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
   - **linq2db.SqlServer** (version 6.1.0)
3. Update **Microsoft.AspNet.Mvc** to version **5.3.0**.

All required packages are now installed. Verify the installation by checking the `packages.config` file in the project root.

---

### Step 4: Create the Data Model

A data model is a C# class that represents the structure of a database table. This model defines the properties that correspond to the columns in the `Tickets` table.

**Instructions:**

1. In the **Solution Explorer**, right-click on the **Models** folder.
2. Select **Add → New Item**.
3. Choose **Class** and name it **Tickets.cs**.
4. Replace the default code with the following:

```csharp
using LinqToDB.Mapping;

namespace MSSQLMVC5Grid.Models
{
    /// <summary>
    /// Represents a support ticket in the NetworkSupportDB database.
    /// This class uses LinqToDB attributes for database mapping.
    /// </summary>
    [Table("Tickets", Schema = "dbo")]
    public sealed class Tickets
    {
        /// <summary>
        /// Primary key. Auto-incremented by the database.
        /// </summary>
        [PrimaryKey, Identity]
        public int TicketId { get; set; }

        /// <summary>
        /// Unique public-facing ticket identifier (e.g., NET-1001).
        /// </summary>
        [Column, NotNull]
        public string PublicTicketId { get; set; }

        /// <summary>
        /// Brief title of the support ticket.
        /// </summary>
        [Column]
        public string Title { get; set; }

        /// <summary>
        /// Detailed description of the issue or request.
        /// </summary>
        [Column]
        public string Description { get; set; }

        /// <summary>
        /// Category classification for the ticket (e.g., Network Issue, Performance, Hardware).
        /// </summary>
        [Column]
        public string Category { get; set; }

        /// <summary>
        /// Department responsible for handling the ticket.
        /// </summary>
        [Column]
        public string Department { get; set; }

        /// <summary>
        /// Name or ID of the person assigned to resolve the ticket.
        /// </summary>
        [Column]
        public string Assignee { get; set; }

        /// <summary>
        /// User who created the ticket.
        /// </summary>
        [Column]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Current status of the ticket (Open, InProgress, Resolved, Closed).
        /// </summary>
        [Column, NotNull]
        public string Status { get; set; }

        /// <summary>
        /// Priority level (Low, Medium, High, Critical).
        /// </summary>
        [Column, NotNull]
        public string Priority { get; set; }

        /// <summary>
        /// Date by which the support team must respond to the ticket.
        /// </summary>
        [Column]
        public System.DateTime? ResponseDue { get; set; }

        /// <summary>
        /// Date by which the issue must be resolved.
        /// </summary>
        [Column]
        public System.DateTime? DueDate { get; set; }

        /// <summary>
        /// Timestamp when the ticket was created (set by database default).
        /// </summary>
        [Column, NotNull]
        public System.DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the ticket was last updated (set by database default).
        /// </summary>
        [Column, NotNull]
        public System.DateTime UpdatedAt { get; set; }
    }
}
```

**Explanation:**

- **`[Table("Tickets", Schema = "dbo")]`**: Maps the class to the `dbo.Tickets` table in the database.
- **`[PrimaryKey, Identity]`**: Marks `TicketId` as the primary key with auto-increment behavior.
- **`[Column]`**: Maps each property to a database column.
- **`[NotNull]`**: Indicates that the column does not allow NULL values.
- **`?` Syntax**: Indicates nullable properties (can be empty).

The data model has been successfully created.

---

### Step 5: Create the AppDataConnection Class

The `AppDataConnection` class serves as the database connection manager using LINQ2DB. This class handles all direct database communication.

**Instructions:**

1. In the **Solution Explorer**, right-click on the **Models** folder.
2. Select **Add → New Item**.
3. Choose **Class** and name it **AppDataConnection.cs**.
4. Replace the default code with the following:

```csharp
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using System.Configuration;

namespace MSSQLMVC5Grid.Models
{
    /// <summary>
    /// Database connection manager for LINQ2DB.
    /// Handles all database operations and connection configuration.
    /// </summary>
    public sealed class AppDataConnection : DataConnection
    {
        /// <summary>
        /// Initializes a new instance of AppDataConnection with SQL Server configuration.
        /// Retrieves the connection string from web.config.
        /// </summary>
        public AppDataConnection() :
            base(new DataOptions()
                .UseSqlServer(
                    ConfigurationManager
                        .ConnectionStrings["DefaultConnection"].ConnectionString,
                    SqlServerVersion.v2019,
                    SqlServerProvider.SystemDataSqlClient))
        {
            // Enable inline parameters for better SQL logging in debug mode.
            // This allows you to see the actual SQL query with parameters in the debugger.
            this.InlineParameters = true;

            // Optional: Enable query tracing for debugging purposes.
            // Uncomment the following line to enable SQL query logging to the Debug output:
            // this.OnTrace = info => System.Diagnostics.Debug.WriteLine(info.SqlText);
        }

        /// <summary>
        /// Provides access to the Tickets table for LINQ2DB queries.
        /// </summary>
        public ITable<Tickets> Tickets => this.GetTable<Tickets>();
    }
}
```

**Explanation:**

- **`AppDataConnection` Class**: Inherits from `LinqToDB.Data.DataConnection`, which manages the database connection lifecycle.
- **Constructor**: Configures the SQL Server connection using the connection string from `web.config`.
- **`InlineParameters`**: When set to `true`, enables inline parameter logging, making SQL debugging easier.
- **`Tickets` Property**: Returns an `ITable<Tickets>` interface that allows LINQ queries against the Tickets table.
- **Connection String**: Retrieved from the `DefaultConnection` entry in `web.config` (configured in Step 6).

The AppDataConnection class is now ready for use in controllers.

---

### Step 6: Configure the Connection String in web.config

A connection string contains the information needed to connect the application to the SQL Server database.

**Instructions:**

1. Open the `web.config` file located in the project root.
2. Find the `<connectionStrings>` section (or create one if it doesn't exist).
3. Add or replace the connection string with the following:

```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NetworkSupportDB;Integrated Security=True;MultipleActiveResultSets=True;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Connection String Components:**

| Component | Description | Example |
|-----------|-------------|---------|
| **Data Source** | The address of the SQL Server instance (server name, IP address, or localhost) | `(localdb)\MSSQLLocalDB` or `localhost` or `SERVER-NAME` |
| **Initial Catalog** | The database name | `NetworkSupportDB` |
| **Integrated Security** | Authentication method (`True` for Windows Auth, `False` for SQL Auth) | `True` (Windows) or `False` (SQL Login) |
| **MultipleActiveResultSets** | Allows multiple concurrent result sets over a single connection | `True` (recommended for web applications) |
| **Connection Timeout** | Connection timeout in seconds (optional) | `30` |
| **Encrypt** | Enables encryption for the connection (optional) | `True` (for production) |
| **TrustServerCertificate** | Trusts the server certificate without validation (optional) | `False` (for security) |

**For Different Authentication Methods:**

- **Windows Authentication (Recommended for Local Development):**
  ```xml
  Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NetworkSupportDB;Integrated Security=True;MultipleActiveResultSets=True;
  ```

- **SQL Server Authentication:**
  ```xml
  Data Source=SERVER-NAME;Initial Catalog=NetworkSupportDB;User ID=sa;Password=YourPassword;MultipleActiveResultSets=True;
  ```

- **Remote Server (SQL Server on a Different Machine):**
  ```xml
  Data Source=192.168.1.100;Initial Catalog=NetworkSupportDB;User ID=sa;Password=YourPassword;MultipleActiveResultSets=True;
  ```

The connection string has been successfully configured.

---

## Implementing the Grid with UrlAdaptor

The **UrlAdaptor** approach delegates all CRUD operations and data processing (filtering, sorting, paging) to the server-side controller. This is the simpler and most common approach for standard data operations.

### Step 7: Create the GridController for UrlAdaptor

The `GridController` handles all HTTP requests from the Syncfusion Grid, including data retrieval, create, update, delete, and batch operations.

**Instructions:**

1. In the **Solution Explorer**, right-click on the **Controllers** folder.
2. Select **Add → Controller**.
3. Choose **MVC 5 Controller - Empty**.
4. Name it **GridController.cs**.
5. Replace the default code with the following:

```csharp
using LinqToDB;
using LinqToDB.Async;
using MSSQLMVC5Grid.Models;
using Syncfusion.EJ2.Base;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MSSQLMVC5Grid.Controllers
{
    /// <summary>
    /// Handles all Syncfusion EJ2 Grid data operations (CRUD and data processing).
    /// Uses LINQ2DB for direct database access and Syncfusion DataManager for data operations.
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
        /// This method is called by the UrlAdaptor for all data retrieval operations.
        /// </summary>
        /// <param name="DataManagerRequest">Contains the details of the data operation requested (filters, sorts, paging, search).</param>
        /// <returns>Returns a JSON object with the filtered, sorted, and paginated data along with the total record count.</returns>
        [HttpPost]
        public async Task<ActionResult> UrlDatasource(DataManagerRequest DataManagerRequest)
        {
            using (var db = new AppDataConnection())
            {
                // Retrieve data from the database using LINQ2DB.
                // The FromSql method executes raw SQL and returns an IQueryable<Tickets>.
                // This IQueryable allows Syncfusion's built-in data operation methods to manipulate the data server-side.
                IQueryable<Tickets> query = db.FromSql<Tickets>(
                    @"SELECT * FROM dbo.Tickets");
                
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

                // Execute the query and retrieve the final dataset.
                var rows = await query.ToListAsync();

                // Return the data and total count to the client.
                // The Grid uses this response to populate and display the records.
                // RequiresCounts determines whether to include the total record count.
                return DataManagerRequest.RequiresCounts ? Json(new { result = rows, count }) : Json(rows);
            }
        }

        // ===== CREATE OPERATION =====
        /// <summary>
        /// Handles the insertion of a new ticket record.
        /// Called when the user adds a new row to the Grid and submits the form.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Insert(Tickets value)
        {
            try
            {
                using (var db = new AppDataConnection())
                {
                    // Insert the new ticket and retrieve the generated identity value.
                    // LINQ2DB automatically handles the database-generated TicketId.
                    var newId = await db.InsertWithInt32IdentityAsync(value);
                    value.TicketId = newId;

                    // Return the created record with its generated ID.
                    // The Grid expects the complete row data in the response.
                    return Json(value);
                }
            }
            catch (Exception ex)
            {
                // Log the exception and return an error response.
                System.Diagnostics.Debug.WriteLine($"Insert Error: {ex.Message}");
                return Json(new { IsError = true, ErrorMessage = ex.Message });
            }
        }

        // ===== UPDATE OPERATION =====
        /// <summary>
        /// Handles the update of an existing ticket record.
        /// Called when the user edits a row and submits the changes.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Update(Tickets value)
        {
            try
            {
                using (var db = new AppDataConnection())
                {
                    // Verify the record exists before attempting to update.
                    bool exists = await db.Tickets.AnyAsync(t => t.TicketId == value.TicketId);
                    if (!exists)
                    {
                        return Json(new { IsError = true, ErrorMessage = "Record not found" });
                    }

                    // Update the record in the database.
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
        /// Handles the deletion of a ticket record.
        /// Called when the user deletes a row from the Grid.
        /// The primary key is passed via the CRUDModel parameter.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Remove(CRUDModel<Tickets> model)
        {
            try
            {
                var key = Convert.ToInt32(model.Key);
                using (var db = new AppDataConnection())
                {
                    // Delete the record by its primary key.
                    var rows = await db.Tickets.DeleteAsync(t => t.TicketId == key);

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
        /// All operations are wrapped in a transaction to ensure data consistency.
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> BatchUpdate(CRUDModel<Tickets> payload)
        {
            try
            {
                using (var db = new AppDataConnection())
                {
                    // Begin a database transaction to ensure atomicity.
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
                                r.TicketId = newId;
                            }
                        }

                        // === BATCH UPDATE ===
                        if (payload.Changed != null && payload.Changed.Count > 0)
                        {
                            foreach (var r in payload.Changed)
                            {
                                // Fine-grained update: explicitly set each column.
                                await db.Tickets
                                    .Where(t => t.TicketId == r.TicketId)
                                    .Set(t => t.PublicTicketId, r.PublicTicketId)
                                    .Set(t => t.Title, r.Title)
                                    .Set(t => t.Description, r.Description)
                                    .Set(t => t.Category, r.Category)
                                    .Set(t => t.Department, r.Department)
                                    .Set(t => t.Assignee, r.Assignee)
                                    .Set(t => t.CreatedBy, r.CreatedBy)
                                    .Set(t => t.Status, r.Status)
                                    .Set(t => t.Priority, r.Priority)
                                    .Set(t => t.ResponseDue, r.ResponseDue)
                                    .Set(t => t.DueDate, r.DueDate)
                                    .UpdateAsync();
                            }
                        }

                        // === BATCH DELETE ===
                        if (payload.Deleted != null && payload.Deleted.Count > 0)
                        {
                            var keys = payload.Deleted.Select(d => d.TicketId).ToArray();
                            await db.Tickets.Where(t => keys.Contains(t.TicketId)).DeleteAsync();
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

**Understanding the UrlAdaptor Controller Flow:**

1. **UrlDatasource Method**: Receives filtering, sorting, and paging requests from the Grid and returns processed data.
2. **QueryableOperation**: Syncfusion's built-in class that applies filters, sorts, searches, and pagination to an `IQueryable<T>` collection.
3. **CRUD Operations**: Each HTTP POST method (Insert, Update, Remove) handles a specific data operation.
4. **Batch Operations**: The BatchUpdate method optimizes performance by handling multiple changes in a single database transaction.

[**SCREENSHOT PLACEHOLDER**: Add screenshot of UrlDatasource method showing the SQL query formation in controller when filtering/sorting is applied. This helps understand how Syncfusion DataManager constructs SQL queries. You can capture this from Visual Studio debugger by setting a breakpoint on the `query` variable after operations are applied.]

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
    ViewBag.Title = "Syncfusion EJ2 Grid - UrlAdaptor";
}

<h2>Support Tickets - UrlAdaptor</h2>

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
        col.Field("TicketId").HeaderText("ID").Width(80).IsPrimaryKey(true).Visible(false).Add();
        col.Field("PublicTicketId").HeaderText("Ticket ID").Width(120).Add();
        col.Field("Title").HeaderText("Title").Width(200).Add();
        col.Field("Description").HeaderText("Description").Width(250).Add();
        col.Field("Category").HeaderText("Category").Width(120).Add();
        col.Field("Department").HeaderText("Department").Width(120).Add();
        col.Field("Assignee").HeaderText("Assignee").Width(120).Add();
        col.Field("Status").HeaderText("Status").Width(100).Add();
        col.Field("Priority").HeaderText("Priority").Width(100).Add();
        col.Field("DueDate").HeaderText("Due Date").Width(120).Type("date").Format("yMd").Add();
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
/// Processes the DataManager request using CustomAdaptor pattern.
/// This method is identical to UrlDatasource but demonstrates the CustomAdaptor approach.
/// Use this when you need to implement custom business logic alongside standard data operations.
/// </summary>
[HttpPost]
public async Task<ActionResult> CustomDatasource(DataManagerRequest DataManagerRequest)
{
    using (var db = new AppDataConnection())
    {
        // === CUSTOM BUSINESS LOGIC ===
        // Example: You can add role-based filtering here.
        // var userRole = User.IsInRole("Admin") ? "Admin" : "User";
        // Based on role, you might filter data differently:
        // if (userRole == "User") query = query.Where(t => t.CreatedBy == User.Identity.Name);

        // Retrieve data from the database.
        IQueryable<Tickets> query = db.FromSql<Tickets>(
            @"SELECT * FROM dbo.Tickets");
        
        // Apply custom filtering based on business logic
        // Example: Filter only high-priority tickets
        // query = query.Where(t => t.Priority == "High" || t.Priority == "Critical");

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
```

This CustomAdaptor method allows you to implement custom business logic (role-based filtering, conditional data processing, etc.) before and after applying Syncfusion's standard data operations.

---

### Step 10: Create the Grid View for CustomAdaptor

**Instructions:**

1. Create a new View file named **CustomIndex.cshtml** in the **Views/Grid** folder.
2. Replace the default code with the following:

```html
@{
    ViewBag.Title = "Syncfusion EJ2 Grid - CustomAdaptor";
}

<h2>Support Tickets - CustomAdaptor</h2>

@(Html.EJ2().Grid<dynamic>()
    .ID("gridCustom")
    .DataSource(dataSource => dataSource
        .Url(Url.Action("CustomDatasource", "Grid"))  // Custom endpoint
        .Adaptor(AdaptorType.UrlAdaptor)              // Use UrlAdaptor (CustomAdaptor uses custom POST methods)
        .CrossDomain(true)
    )
    .AllowPaging()
    .AllowSorting()
    .AllowSearching()
    .AllowFiltering(filter => filter.Type(FilterType.Menu))
    .Columns(col =>
    {
        col.Field("TicketId").HeaderText("ID").Width(80).IsPrimaryKey(true).Visible(false).Add();
        col.Field("PublicTicketId").HeaderText("Ticket ID").Width(120).Add();
        col.Field("Title").HeaderText("Title").Width(200).Add();
        col.Field("Description").HeaderText("Description").Width(250).Add();
        col.Field("Category").HeaderText("Category").Width(120).Add();
        col.Field("Department").HeaderText("Department").Width(120).Add();
        col.Field("Assignee").HeaderText("Assignee").Width(120).Add();
        col.Field("Status").HeaderText("Status").Width(100).Add();
        col.Field("Priority").HeaderText("Priority").Width(100).Add();
        col.Field("DueDate").HeaderText("Due Date").Width(120).Type("date").Format("yMd").Add();
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

### Issue 2: LINQ2DB vs Entity Framework - When to Use Each

**Problem:**

When should I use LINQ2DB vs Entity Framework (EF)? What's the difference?

**Solution:**

| Aspect | LINQ2DB | Entity Framework |
|--------|---------|------------------|
| **Learning Curve** | Lower (closer to LINQ) | Higher (more abstractions) |
| **Performance** | Very fast (minimal overhead) | Slightly slower (more features) |
| **Features** | Basic CRUD, simple relationships | Advanced (complex relationships, migrations, lazy loading) |
| **Database Migrations** | Manual SQL scripts required | Built-in migration support |
| **Configuration** | Simple (connection string only) | Complex (DbContext, OnModelCreating) |
| **Use Case** | Simple to moderate data operations | Complex enterprise applications |
| **Syncfusion Compatibility** | Excellent (lightweight) | Good (but heavier) |

**For Syncfusion Grid Integration:**

- **Use LINQ2DB** if:
  - You want lightweight, fast performance.
  - Your database schema is already established.
  - You need simple CRUD + filtering/sorting/paging.
  - You want minimal configuration.

- **Use Entity Framework** if:
  - You need complex business logic and relationship navigation.
  - You want automatic database migrations.
  - You're building a large-scale enterprise application.
  - Syncfusion has provided dedicated EF documentation (see separate EF guide).

**For this guide, LINQ2DB is recommended because:**

1. It's lightweight and performant for web grid scenarios.
2. Syncfusion's QueryableOperation integrates seamlessly with LINQ2DB's `IQueryable<T>`.
3. It requires minimal configuration compared to Entity Framework.
4. It's sufficient for standard CRUD and data grid operations.

---

### Issue 3: SQL Server Connection String Issues

**Problem:**
```
A network-related or instance-specific error occurred while establishing a connection to SQL Server.
```

or

```
Connection timeout expired. The timeout period elapsed prior to completion of the operation.
```

**Root Causes and Solutions:**

#### 3a: Incorrect Server Name

**Diagnosis:**
- Check the `Data Source` in your connection string.

**Solutions:**

For **Local Development with LocalDB:**
```xml
Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NetworkSupportDB;Integrated Security=True;
```

For **Local SQL Server Express Instance:**
```xml
Data Source=.\SQLEXPRESS;Initial Catalog=NetworkSupportDB;Integrated Security=True;
```

For **Named SQL Server Instance:**
```xml
Data Source=COMPUTER-NAME\INSTANCE-NAME;Initial Catalog=NetworkSupportDB;Integrated Security=True;
```

For **Remote Server by IP Address:**
```xml
Data Source=192.168.1.100;Initial Catalog=NetworkSupportDB;User ID=sa;Password=YourPassword;
```

#### 3b: Database Does Not Exist

**Diagnosis:**
- Verify that the database `NetworkSupportDB` has been created in SQL Server.

**Solution:**
```sql
-- In SQL Server Management Studio, execute:
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'NetworkSupportDB')
BEGIN
    CREATE DATABASE NetworkSupportDB;
END
```

#### 3c: Integrated Security / Windows Authentication Failed

**Diagnosis:**
- You're using Windows Authentication but the application doesn't have permission.

**Solutions:**

- **For Local Development**, ensure the current user is running Visual Studio with appropriate permissions.
- **For IIS/Production**, ensure the IIS application pool identity has access to SQL Server:
  ```sql
  -- In SQL Server Management Studio, create a login and user:
  CREATE LOGIN [DOMAIN\IIS-AppPoolIdentity] FROM WINDOWS;
  CREATE USER [DOMAIN\IIS-AppPoolIdentity] FOR LOGIN [DOMAIN\IIS-AppPoolIdentity];
  EXEC sp_addrolemember 'db_datareader', 'DOMAIN\IIS-AppPoolIdentity';
  EXEC sp_addrolemember 'db_datawriter', 'DOMAIN\IIS-AppPoolIdentity';
  ```

#### 3d: SQL Server Authentication - Incorrect Password

**Diagnosis:**
- You're using SQL Authentication (User ID and Password).

**Solution:**
```xml
<!-- Verify credentials in connection string -->
Data Source=SERVER-NAME;Initial Catalog=NetworkSupportDB;User ID=sa;Password=YourPassword;MultipleActiveResultSets=True;
```

#### 3e: Connection Timeout

**Diagnosis:**
- The server is reachable but responds slowly or is overloaded.

**Solution:**
```xml
<!-- Add timeout to connection string -->
Data Source=SERVER-NAME;Initial Catalog=NetworkSupportDB;Integrated Security=True;Connection Timeout=30;
```

Increase the value (default is 15 seconds) if the server is slow.

---

### Issue 4: "The LINQ2DB Mapping Is Incorrect" Error

**Problem:**
```
Execution timeout expired. The timeout period elapsed prior to completion of the operation or the operation has been cancelled.
```

or

```
No columns matched for entity mapping
```

**Root Cause:**

The `[Table]` or `[Column]` attributes in the `Tickets` model class don't match the actual database schema.

**Solution:**

1. Verify the exact table and column names in SQL Server:
   ```sql
   EXEC sp_columns 'Tickets';
   ```

2. Update the `Tickets.cs` model to match exactly:
   ```csharp
   [Table("Tickets", Schema = "dbo")]  // Must match database table name
   public sealed class Tickets
   {
       [Column]
       public string PublicTicketId { get; set; }  // Column names must match database exactly
   }
   ```

3. Ensure all properties are decorated with `[Column]`.

---

### Issue 5: Grid Not Loading Data

**Problem:**

The Grid appears empty or shows no data even though the database has records.

**Diagnosis Steps:**

1. **Enable Console Logging:** Open browser DevTools (F12) and check the Console tab for JavaScript errors.

2. **Check Network Requests:** In DevTools, go to **Network** tab and look for the request to `UrlDatasource`. Verify:
   - Status code is 200 (success).
   - Response contains JSON data.

3. **Check Server Logs:** In Visual Studio, check the **Output** window for any exceptions.

**Common Solutions:**

1. **Verify the controller endpoint URL:**
   ```csharp
   .Url(Url.Action("UrlDatasource", "Grid"))  // Correct controller and method name
   ```

2. **Ensure the Grid ID matches:**
   ```html
   @(Html.EJ2().Grid<dynamic>().ID("grid") ...)  <!-- ID must match JavaScript references -->
   ```

3. **Enable debugging:** Add breakpoints in the controller's `UrlDatasource` method to verify it's being called.

4. **Check CORS if applicable:** If the data endpoint is on a different domain:
   ```csharp
   .CrossDomain(true)  // Enable cross-domain requests
   ```

---

### Issue 6: Syncfusion License Warning

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

### Issue 7: "Data Type Mismatch" in Update/Insert Operations

**Problem:**
```
Error converting data type varchar to datetime.
```

or

```
Conversion failed when converting from a character string to smalldatetime.
```

**Root Cause:**

The Grid is sending data in a format that doesn't match the database column type.

**Solutions:**

1. **For DateTime columns:** Ensure the Grid format matches the expected format:
   ```csharp
   col.Field("DueDate")
       .HeaderText("Due Date")
       .Type("date")
       .Format("yMd")  // Format: Year-Month-Day
       .Add();
   ```

2. **In the controller, validate data types before insertion:**
   ```csharp
   [HttpPost]
   public async Task<ActionResult> Insert(Tickets value)
   {
       if (value.DueDate.HasValue && value.DueDate < DateTime.Now)
       {
           return Json(new { IsError = true, ErrorMessage = "Due date cannot be in the past" });
       }
       // ... proceed with insertion
   }
   ```

3. **Map DateTime columns correctly in the model:**
   ```csharp
   [Column]
   public System.DateTime? DueDate { get; set; }  // Nullable DateTime
   ```

---

## Summary

You have successfully created an ASP.NET MVC5 application with:

✅ SQL Server database setup with the `Tickets` table  
✅ LINQ2DB for lightweight, efficient database operations  
✅ Syncfusion EJ2 Grid with **UrlAdaptor** for standard CRUD operations  
✅ Syncfusion EJ2 Grid with **CustomAdaptor** for custom business logic  
✅ Full support for filtering, sorting, searching, paging, and batch operations  
✅ Error handling and troubleshooting guidelines  

### Next Steps

1. **Test the application** by running it in Visual Studio (F5).
2. **Verify CRUD operations** by adding, editing, and deleting records.
3. **Customize the Grid columns** based on your specific business requirements.
4. **Implement role-based filtering** in the CustomAdaptor for advanced scenarios.
5. **Deploy to production** following your organization's deployment procedures.

### Additional Resources

- [Syncfusion EJ2 ASP.NET MVC Grid Documentation](https://www.syncfusion.com/aspnet-mvc-components/mvc-grid)
- [LINQ2DB Official Documentation](https://linq2db.github.io/)
- [ASP.NET MVC 5 Official Documentation](https://docs.microsoft.com/en-us/aspnet/mvc/mvc5)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/sql-server/)

---

**Documentation Version:** 1.0  
**Last Updated:** February 1, 2026  
**Compatible with:** Syncfusion 32.1.19, ASP.NET MVC 5.3.0, LINQ2DB 6.1.0, .NET Framework 4.8.1
