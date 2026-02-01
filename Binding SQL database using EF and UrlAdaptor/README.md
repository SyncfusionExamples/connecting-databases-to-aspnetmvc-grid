---
layout: post
title: ASP.NET MVC5 Data Grid connected to SQL Server via Entity Framework | Syncfusion
description: Bind SQL Server data to ASP.NET MVC5 Data Grid using Entity Framework 6 with complete CRUD, filtering, sorting, paging, and advanced data operations using UrlAdaptor and CustomAdaptor patterns.
platform: ASP.NET MVC5
control: DataGrid (EJ2 Grid)
documentation: ug
---

# Connecting SQL Server to ASP.NET MVC5 Data Grid Using Entity Framework

The [Syncfusion<sup style="font-size:70%">&reg;</sup> EJ2 ASP.NET MVC5 DataGrid](https://www.syncfusion.com/aspnet-mvc-components/mvc-grid) supports binding data from a SQL Server database. This documentation demonstrates how to integrate SQL Server with Syncfusion EJ2 Grid using **Entity Framework 6 (EF6)** for data operations with both **UrlAdaptor** and **CustomAdaptor** approaches.

**What is Entity Framework?**

Entity Framework (EF) is a modern object-relational mapper (ORM) for .NET that simplifies database operations. It serves as a bridge between C# code and SQL Server, eliminating the need for raw SQL queries and providing a more intuitive, code-first approach to database management.

**Key Benefits of Entity Framework for Syncfusion Grid Integration**

- **Change Tracking**: Automatically detects and tracks changes to entities for persistence, simplifying CRUD operations.
- **LINQ Support**: Use familiar LINQ syntax for type-safe database queries.
- **Built-in Security**: Automatic parameterization prevents SQL injection attacks.
- **Database Migrations**: Manage schema changes version-by-version without manual SQL scripts.
- **Lazy Loading & Eager Loading**: Control entity relationship loading for optimized performance.
- **Fluent Configuration**: Fine-grained entity mapping configuration in code.
- **Compatibility with Syncfusion DataManager**: Works seamlessly with Syncfusion EJ2 Grid's DataOperations for filtering, sorting, paging, and searching.

**Entity Framework vs LINQ2DB - Which One to Use?**

| Aspect | Entity Framework 6 | LINQ2DB |
|--------|---|---|
| **Learning Curve** | Moderate (more complex abstractions) | Lower (closer to LINQ) |
| **Performance** | Good (with optimization) | Very fast (minimal overhead) |
| **Features** | Comprehensive (migrations, lazy loading, change tracking) | Basic (simple CRUD, data operations) |
| **Configuration** | Complex (DbContext, OnModelCreating, migrations) | Simple (connection string only) |
| **Async/Await Support** | Native async throughout | Full async support |
| **Database Migrations** | Built-in with EF Migrations | Manual SQL scripts required |
| **Use Case** | Complex business logic, enterprise apps, long-term projects | Simple CRUD, web grids, lightweight scenarios |
| **Syncfusion Integration** | Good (this documentation covers MVC5 EF6) | Excellent (lightweight alternative) |

**Recommendation:**
- **Use Entity Framework** if: Your application requires complex business logic, multiple related entities, database migrations, and long-term maintainability.
- **Use LINQ2DB** if: You need lightweight, fast performance for simple CRUD operations with minimal configuration overhead.

**What is the Difference Between UrlAdaptor and CustomAdaptor?**

| Aspect | UrlAdaptor | CustomAdaptor |
|--------|-----------|---------------|
| **Data Processing** | Server-side via controller action | Custom server-side logic |
| **HTTP Method** | POST requests to controller actions | POST/GET custom methods |
| **Use Case** | Standard CRUD operations | Complex business logic, custom validation |
| **Built-in DataManager** | Uses Syncfusion's QueryableOperation for filtering/sorting/paging | Manual implementation of data operations |
| **Configuration** | Simple URL mapping in Grid configuration | Custom ActionResult methods |

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
| EntityFramework | 6.5.1 or later | Object-Relational Mapper for database operations |
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
   - **Project name**: `EFMVC5Grid` (or your preferred name)
   - **Location**: Choose your desired folder
   - **Framework**: Select **.NET Framework 4.8.1**
6. Click **Create**.
7. In the **Create a new ASP.NET Web Application** dialog:
   - Select **MVC** template.
   - Ensure **Authentication** is set to **No Authentication** for simplicity.
   - Click **Create**.

Visual Studio will create the project with the default MVC5 structure, including folders like **Controllers**, **Models**, **Views**, and configuration files. The ASP.NET MVC5 project is now ready for integration with Entity Framework and Syncfusion components.

---

### Step 3: Install Required NuGet Packages

NuGet packages are software libraries that add functionality to the application. These packages enable Entity Framework, SQL Server connectivity, and Syncfusion Grid integration.

**Method 1: Using Package Manager Console (Recommended)**

1. Open Visual Studio 2022.
2. Navigate to **Tools → NuGet Package Manager → Package Manager Console**.
3. Run the following commands in sequence:

```powershell
Install-Package Syncfusion.EJ2.MVC5 -Version 32.1.19
Install-Package Syncfusion.EJ2.JavaScript -Version 32.1.19
Install-Package Syncfusion.Licensing -Version 32.1.19
Install-Package EntityFramework -Version 6.5.1
Update-Package Microsoft.AspNet.Mvc -Version 5.3.0
```

**Important:** The last command updates Microsoft.AspNet.Mvc from the default 5.2.9 to 5.3.0 for compatibility with Syncfusion packages.

**Method 2: Using NuGet Package Manager UI**

1. Open **Visual Studio 2022 → Tools → NuGet Package Manager → Manage NuGet Packages for Solution**.
2. Search for and install each package individually:
   - **Syncfusion.EJ2.MVC5** (version 32.1.19)
   - **Syncfusion.EJ2.JavaScript** (version 32.1.19)
   - **Syncfusion.Licensing** (version 32.1.19)
   - **EntityFramework** (version 6.5.1)
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
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFMVC5Grid.Models
{
    /// <summary>
    /// Represents a support ticket record mapped to the 'Tickets' table in SQL Server.
    /// This model defines the structure of ticket-related data used throughout the application.
    /// Entity Framework 6 uses these annotations for database mapping and validation.
    /// </summary>
    [Table("Tickets", Schema = "dbo")]
    public class Tickets
    {
        /// <summary>
        /// Primary key. Auto-incremented by the database via IDENTITY(1,1).
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketId { get; set; }

        /// <summary>
        /// Public-facing ticket identifier (e.g., NET-1001).
        /// Required, Unique, VARCHAR(50) NOT NULL.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Index("IX_Tickets_PublicTicketId", IsUnique = true)]
        public string PublicTicketId { get; set; }

        /// <summary>
        /// Brief title of the support ticket.
        /// VARCHAR(200) NULL.
        /// </summary>
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Detailed description of the issue or request.
        /// TEXT NULL (stored as nvarchar(max) in EF).
        /// </summary>
        [Column(TypeName = "text")]
        public string Description { get; set; }

        /// <summary>
        /// Category classification for the ticket (e.g., Network Issue, Hardware).
        /// VARCHAR(100) NULL.
        /// </summary>
        [MaxLength(100)]
        public string Category { get; set; }

        /// <summary>
        /// Department responsible for handling the ticket.
        /// VARCHAR(100) NULL.
        /// </summary>
        [MaxLength(100)]
        public string Department { get; set; }

        /// <summary>
        /// Name or ID of the person assigned to resolve the ticket.
        /// VARCHAR(100) NULL.
        /// </summary>
        [MaxLength(100)]
        public string Assignee { get; set; }

        /// <summary>
        /// User who created the ticket.
        /// VARCHAR(100) NULL.
        /// </summary>
        [MaxLength(100)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Current status of the ticket (Open, InProgress, Resolved, Closed).
        /// VARCHAR(50) NOT NULL, DEFAULT 'Open'.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        /// <summary>
        /// Priority level (Low, Medium, High, Critical).
        /// VARCHAR(50) NOT NULL, DEFAULT 'Medium'.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Priority { get; set; }

        /// <summary>
        /// Date by which the support team must respond to the ticket.
        /// DATETIME2 NULL.
        /// </summary>
        public DateTime? ResponseDue { get; set; }

        /// <summary>
        /// Date by which the issue must be resolved.
        /// DATETIME2 NULL.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Timestamp when the ticket was created (set by database DEFAULT GETDATE()).
        /// DATETIME2 NOT NULL.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the ticket was last updated (set by database DEFAULT GETDATE()).
        /// DATETIME2 NOT NULL.
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}
```

**Explanation:**

- **`[Table("Tickets", Schema = "dbo")]`**: Maps the class to the `dbo.Tickets` table in the database.
- **`[Key]`**: Marks `TicketId` as the primary key.
- **`[DatabaseGenerated(DatabaseGeneratedOption.Identity)]`**: Indicates auto-increment behavior via IDENTITY(1,1).
- **`[Required]`**: Indicates that a column does not allow NULL values.
- **`[MaxLength(n)]`**: Specifies the maximum length of a VARCHAR column.
- **`[Index(..., IsUnique = true)]`**: Creates a unique index on the PublicTicketId column (EF 6.1+).
- **`[Column(TypeName = "text")]`**: Explicitly specifies the SQL Server column type as TEXT.

The data model has been successfully created.

---

### Step 5: Create the DbContext Class

The `DbContext` is a special class that manages the connection between the application and the SQL Server database. It handles all database operations such as saving, updating, deleting, and retrieving data.

**Instructions:**

1. In the **Solution Explorer**, right-click on the **Models** folder.
2. Select **Add → New Item**.
3. Choose **Class** and name it **TicketsDbContext.cs**.
4. Replace the default code with the following:

```csharp
using System.Data.Entity;

namespace EFMVC5Grid.Models
{
    /// <summary>
    /// DbContext for managing Tickets entity and database operations.
    /// Inherits from Entity Framework's DbContext to handle all database interactions
    /// for the Network Support Ticket System.
    /// </summary>
    public class TicketsDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of TicketsDbContext.
        /// Retrieves the connection string from web.config using the name "TicketsDbContext".
        /// Enables SQL logging to the debug output for debugging purposes.
        /// </summary>
        public TicketsDbContext()
            : base("name=TicketsDbContext")
        {
            // Enable SQL query logging to debug output for development and debugging.
            // This shows the actual SQL being executed, helpful for optimization.
            this.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }

        /// <summary>
        /// DbSet for the Tickets entity.
        /// Provides access to Ticket records in the database for querying and persistence.
        /// </summary>
        public DbSet<Tickets> Tickets => Set<Tickets>();

        /// <summary>
        /// Configures entity mappings, relationships, and constraints.
        /// This method is called by Entity Framework during model creation to configure
        /// how entities map to database tables and how their properties map to columns.
        /// </summary>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Tickets entity
            var entity = modelBuilder.Entity<Tickets>();

            // Primary Key configuration
            entity.HasKey(e => e.TicketId);

            // Auto-increment for Primary Key
            entity.Property(e => e.TicketId)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);

            // Column configurations
            entity.Property(e => e.PublicTicketId)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsOptional();

            entity.Property(e => e.Description)
                .IsOptional();

            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsOptional();

            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .IsOptional();

            entity.Property(e => e.Assignee)
                .HasMaxLength(100)
                .IsOptional();

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsOptional();

            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Priority)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.ResponseDue)
                .IsOptional();

            entity.Property(e => e.DueDate)
                .IsOptional();

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
        }
    }
}
```

**Explanation:**

- **`DbContext` Class**: Inherits from Entity Framework's `DbContext` base class, managing the database connection and entity tracking.
- **Constructor**: Calls the base constructor with the connection string name "TicketsDbContext" from `web.config`.
- **`Database.Log`**: Enables SQL query logging to the debug output, allowing you to see the SQL being executed.
- **`Tickets` Property**: Returns a `DbSet<Tickets>` for querying and persisting Ticket records.
- **`OnModelCreating`**: Configures entity mappings, such as primary keys, maximum lengths, required fields, and data types.

The **TicketsDbContext** class is essential because:

- It **connects** the application to the SQL Server database.
- It **manages** the entity lifecycle (Add, Update, Delete, SaveChanges).
- It **maps** C# models to actual database tables and columns.
- It **configures** how data should look inside the database.
- It **enables** SQL Server-specific features like identity columns and datetime2 types.

Without this class, Entity Framework cannot manage database operations. The DbContext has been successfully configured.

---

### Step 6: Configure the Connection String in web.config

A connection string contains the information needed to connect the application to the SQL Server database.

**Instructions:**

1. Open the `web.config` file located in the project root.
2. Find the `<connectionStrings>` section (or create one if it doesn't exist).
3. Add the `<configSections>` entry for Entity Framework and the connection string with the following:

```xml
<!-- Add this in the <configuration> opening tag -->
<configSections>
    <!-- For more information on Entity Framework configuration, visit http://go.microsoft.com/fwlink/?LinkID=237468 -->
    <section name="entityFramework" type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
</configSections>

<!-- Add this at the end, before </configuration> -->
<connectionStrings>
    <add name="TicketsDbContext" 
         connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NetworkSupportDB;Integrated Security=True;MultipleActiveResultSets=True;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>

<!-- Add this at the end for Entity Framework configuration -->
<entityFramework>
    <defaultConnectionFactory type="System.Data.Entity.Infrastructure.SqlConnectionFactory, EntityFramework" />
    <providers>
        <provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer" />
    </providers>
</entityFramework>
```

**Connection String Components:**

| Component | Description | Example |
|-----------|-------------|---------|
| **Data Source** | The address of the SQL Server instance (server name, IP address, or localhost) | `(localdb)\MSSQLLocalDB` or `localhost` or `SERVER-NAME` |
| **Initial Catalog** | The database name | `NetworkSupportDB` |
| **Integrated Security** | Authentication method (`True` for Windows Auth, `False` for SQL Auth) | `True` (Windows) or `False` (SQL Login) |
| **MultipleActiveResultSets** | Allows multiple concurrent result sets over a single connection | `True` (recommended for web applications) |
| **providerName** | The connection provider for EF | `System.Data.SqlClient` |

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

The connection string and Entity Framework configuration have been successfully set up.

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
using EFMVC5Grid.Models;
using Syncfusion.EJ2.Base;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace EFMVC5Grid.Controllers
{
    /// <summary>
    /// Handles all Syncfusion EJ2 Grid data operations (CRUD and data processing).
    /// Uses Entity Framework 6 for database access and Syncfusion DataManager for data operations.
    /// Implements the UrlAdaptor pattern for server-side data processing.
    /// </summary>
    public class GridController : Controller
    {
        // DbContext instance for Entity Framework database operations
        private readonly TicketsDbContext _db;

        /// <summary>
        /// Initializes a new instance of the GridController.
        /// Creates a new TicketsDbContext instance using the connection string from web.config.
        /// </summary>
        public GridController()
        {
            _db = new TicketsDbContext();
        }

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
        /// <param name="DataManagerRequest">Contains the details of the data operation requested.</param>
        /// <returns>Returns a JSON object with the filtered, sorted, and paginated data along with the total record count.</returns>
        [HttpPost]
        public ActionResult UrlDatasource(DataManagerRequest DataManagerRequest)
        {
            try
            {
                // Retrieve data from the database using Entity Framework.
                // AsNoTracking() is used to prevent EF from tracking entities, improving performance for read-only queries.
                IQueryable<Tickets> query = _db.Tickets.AsNoTracking();
                
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
                else
                {
                    // EF6 requires an OrderBy before Skip for pagination.
                    // Use a stable primary key for consistent ordering.
                    query = query.OrderBy(t => t.TicketId);
                }

                // === TOTAL COUNT ===
                // Get the total number of records after filtering but before paging.
                // This is used by the Grid to display pagination information.
                int count = query.Count();

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

                // Return the data and total count to the client.
                // The Grid uses this response to populate and display the records.
                // RequiresCounts determines whether to include the total record count.
                return DataManagerRequest.RequiresCounts 
                    ? Json(new { result = query.ToList(), count }) 
                    : Json(query.ToList());
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
        /// Handles the insertion of a new ticket record.
        /// Called when the user adds a new row to the Grid and submits the form.
        /// </summary>
        [HttpPost]
        public ActionResult Insert(Tickets value)
        {
            try
            {
                // Add the new ticket to the DbContext.
                _db.Tickets.Add(value);
                
                // Save changes to the database.
                // EF automatically assigns the generated TicketId from the identity column.
                _db.SaveChanges();

                // Return the created record with its generated ID.
                // The Grid expects the complete row data in the response.
                return Json(value);
            }
            catch (Exception ex)
            {
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
        public ActionResult Update(Tickets value)
        {
            try
            {
                // Verify the record exists before attempting to update.
                bool exists = _db.Tickets.Any(t => t.TicketId == value.TicketId);
                if (!exists)
                {
                    return Json(new { IsError = true, ErrorMessage = "Record not found" });
                }

                // Mark the entity as modified and save changes.
                // EF Change Tracking automatically detects what properties have changed.
                _db.Entry(value).State = EntityState.Modified;
                _db.SaveChanges();

                // Return the updated record.
                return Json(value);
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
        public ActionResult Remove(CRUDModel<Tickets> model)
        {
            try
            {
                var key = Convert.ToInt32(model.Key);
                
                // Find the record by its primary key.
                var entity = _db.Tickets.FirstOrDefault(t => t.TicketId == key);
                if (entity == null)
                {
                    return Json(new { IsError = true, ErrorMessage = "Record not found" });
                }

                // Remove the record from the DbContext and save changes.
                _db.Tickets.Remove(entity);
                _db.SaveChanges();

                // Return the model to confirm the deletion.
                return Json(model);
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
        /// All operations are wrapped in a database transaction to ensure data consistency.
        /// </summary>
        [HttpPost]
        public JsonResult BatchUpdate(CRUDModel<Tickets> value)
        {
            try
            {
                // === BATCH UPDATE ===
                if (value.Changed != null && value.Changed.Count > 0)
                {
                    foreach (Tickets record in value.Changed)
                    {
                        // Mark the entity as modified for EF tracking.
                        _db.Tickets.Attach(record);
                        _db.Entry(record).State = EntityState.Modified;
                    }
                }

                // === BATCH INSERT ===
                if (value.Added != null && value.Added.Count > 0)
                {
                    foreach (Tickets ticket in value.Added)
                    {
                        // Ensure EF does not try to insert the TicketId (it's auto-generated).
                        ticket.TicketId = default(int);
                        
                        // Add new records to the DbContext.
                        _db.Tickets.Add(ticket);
                    }
                }

                // === BATCH DELETE ===
                if (value.Deleted != null && value.Deleted.Count > 0)
                {
                    foreach (Tickets record in value.Deleted)
                    {
                        // Find and delete the records using the primary key.
                        var existingTicket = _db.Tickets.Find(record.TicketId);
                        if (existingTicket != null)
                        {
                            _db.Tickets.Remove(existingTicket);
                        }
                    }
                }

                // Save all changes to the database in a single transaction.
                // EF handles the transaction implicitly; all changes are saved together.
                _db.SaveChanges();
                
                return Json(value, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Batch Update Exception: {ex.Message}");
                return Json(new { IsError = true, ErrorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Dispose the DbContext to free resources.
        /// This ensures the database connection is properly closed and resources are released.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
```

**Understanding the UrlAdaptor Controller Flow with Entity Framework:**

1. **UrlDatasource Method**: Receives filtering, sorting, and paging requests from the Grid and returns processed data.
2. **QueryableOperation**: Syncfusion's built-in class that applies filters, sorts, searches, and pagination to an `IQueryable<T>` collection.
3. **Entity Framework Change Tracking**: Automatically tracks changes to entities and persists them when `SaveChanges()` is called.
4. **CRUD Operations**: Each HTTP POST method (Insert, Update, Remove) handles a specific data operation using EF's API.
5. **Batch Operations**: The BatchUpdate method optimizes performance by handling multiple changes together.
6. **AsNoTracking()**: Used in read-only queries to improve performance by preventing EF from tracking entity changes.

[**SCREENSHOT PLACEHOLDER**: Add screenshot of UrlDatasource method showing the SQL query formation in controller when filtering/sorting is applied. This helps understand how Entity Framework constructs SQL queries behind the scenes. You can capture this from Visual Studio debugger by enabling `Database.Log` and observing the debug output, or by using SQL Server Profiler to see the actual SQL being executed.]

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
    ViewBag.Title = "Syncfusion EJ2 Grid - UrlAdaptor with Entity Framework";
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

For the CustomAdaptor pattern, add the following method to the existing **GridController.cs**:

```csharp
/// <summary>
/// Processes the DataManager request using CustomAdaptor pattern.
/// This method demonstrates custom business logic implementation for data operations.
/// Use this when you need to implement custom filtering, validation, or business rules
/// alongside standard Syncfusion data operations.
/// </summary>
[HttpPost]
public ActionResult CustomDatasource(DataManagerRequest DataManagerRequest)
{
    try
    {
        // === CUSTOM BUSINESS LOGIC ===
        // Example: You can implement role-based filtering, status-based filtering, etc.
        
        // Example 1: Filter only high-priority tickets
        // var query = _db.Tickets.Where(t => t.Priority == "High" || t.Priority == "Critical");
        
        // Example 2: Filter by date range
        // var startDate = new DateTime(2026, 01, 01);
        // var query = _db.Tickets.Where(t => t.CreatedAt >= startDate);

        // Start with all tickets
        IQueryable<Tickets> query = _db.Tickets.AsNoTracking();
        
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
        else
        {
            query = query.OrderBy(t => t.TicketId);
        }

        int count = query.Count();

        if (DataManagerRequest.Skip != 0)
        {
            query = operation.PerformSkip(query, DataManagerRequest.Skip);
        }

        if (DataManagerRequest.Take != 0)
        {
            query = operation.PerformTake(query, DataManagerRequest.Take);
        }

        return DataManagerRequest.RequiresCounts 
            ? Json(new { result = query.ToList(), count }) 
            : Json(query.ToList());
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"CustomDatasource Error: {ex.Message}");
        return Json(new { IsError = true, ErrorMessage = ex.Message });
    }
}
```

This CustomAdaptor method allows you to implement custom business logic (role-based filtering, complex validation, custom calculations, etc.) before and after applying Syncfusion's standard data operations.

---

### Step 10: Create the Grid View for CustomAdaptor

**Instructions:**

1. Create a new View file named **CustomIndex.cshtml** in the **Views/Grid** folder.
2. Replace the default code with the following:

```html
@{
    ViewBag.Title = "Syncfusion EJ2 Grid - CustomAdaptor with Entity Framework";
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

### Issue 2: Entity Framework vs LINQ2DB - When to Use Each

**Problem:**

When should I use Entity Framework vs LINQ2DB? What's the difference?

**Solution:**

| Aspect | Entity Framework 6 | LINQ2DB |
|--------|---|---|
| **Learning Curve** | Moderate | Lower |
| **Configuration Complexity** | High (DbContext, migrations, OnModelCreating) | Low (connection string only) |
| **Performance** | Good (with optimization) | Very fast |
| **Features** | Comprehensive (migrations, lazy loading, change tracking) | Basic (CRUD + QueryableOperation) |
| **Async Support** | Native async/await throughout | Full async support |
| **Database Migrations** | Built-in with EF Migrations | Manual SQL scripts |
| **Change Tracking** | Automatic (can be disabled with AsNoTracking()) | Manual |
| **Relationship Navigation** | Automatic lazy/eager loading | Manual LINQ queries |
| **Use Case** | Complex enterprise apps, multi-table relationships | Simple CRUD, data grids, lightweight apps |

**For Syncfusion Grid Integration:**

- **Use Entity Framework** if:
  - Your application has complex business logic requiring multiple related entities.
  - You need automatic database migrations for schema evolution.
  - You want change tracking and navigation properties.
  - Long-term maintainability is a priority.

- **Use LINQ2DB** if:
  - You need lightweight, fast performance for simple CRUD + data grid operations.
  - You want minimal configuration and overhead.
  - Your database schema is stable and predefined.
  - You prefer simplicity over advanced features.

**For this documentation**, Entity Framework 6 is recommended for:
1. Mature projects with complex data relationships.
2. Applications requiring database migrations and version control.
3. Teams familiar with EF patterns and change tracking.
4. Long-term projects where maintainability outweighs performance overhead.

---

### Issue 3: Entity Framework DbContext Connection String Issues

**Problem:**
```
The entity type 'Tickets' is not part of the model for the current context.
```

or

```
A network-related or instance-specific error occurred while establishing a connection to SQL Server.
```

**Root Cause:**

The connection string name in the DbContext constructor doesn't match the name in `web.config`, or the connection string is incorrect.

**Solution:**

1. **Verify the connection string name matches:**

   In **TicketsDbContext.cs**:
   ```csharp
   public TicketsDbContext()
       : base("name=TicketsDbContext")  // Name must match web.config
   ```

   In **web.config**:
   ```xml
   <connectionStrings>
       <add name="TicketsDbContext" connectionString="..." />
   </connectionStrings>
   ```

2. **Test the connection string:**
   ```csharp
   using (var db = new TicketsDbContext())
   {
       var test = db.Database.Connection.ConnectionString;
       System.Diagnostics.Debug.WriteLine(test);
   }
   ```

3. **Verify the database exists:**
   - Open SQL Server Management Studio.
   - Ensure the `NetworkSupportDB` database is created.

---

### Issue 4: Entity Framework Lazy Loading Issues

**Problem:**
```
The relationship between types cannot be changed because it is not open.
```

or

```
LINQ to Entities does not recognize the method, and this method cannot be translated into a store expression.
```

**Root Cause:**

Entity Framework tried to lazy-load a navigation property that doesn't exist in the query.

**Solution:**

Use `AsNoTracking()` for read-only queries to prevent lazy loading:

```csharp
IQueryable<Tickets> query = _db.Tickets.AsNoTracking();
```

Or use `Include()` for eager loading if you need related data:

```csharp
IQueryable<Tickets> query = _db.Tickets.Include(t => t.RelatedEntity).AsNoTracking();
```

---

### Issue 5: Grid Not Loading Data

**Problem:**

The Grid appears empty or shows no data even though the database has records.

**Diagnosis Steps:**

1. **Enable Console Logging:** Open browser DevTools (F12) and check the Console tab for JavaScript errors.

2. **Check Network Requests:** In DevTools, go to **Network** tab and look for the request to `UrlDatasource`. Verify:
   - Status code is 200 (success).
   - Response contains JSON data.

3. **Check Server Logs:** In Visual Studio, check the **Output** window for exceptions. Also check the **Database.Log** output for SQL queries.

4. **Enable SQL Logging:** Set breakpoints in the controller and check the debug output for generated SQL:
   ```csharp
   // In TicketsDbContext constructor
   this.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
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

3. **Enable debugging:** Add breakpoints in the controller's `UrlDatasource` method to verify it's being called with data from the database.

4. **Check CORS if applicable:** If the data endpoint is on a different domain:
   ```csharp
   .CrossDomain(true)  // Enable cross-domain requests
   ```

---

### Issue 6: Entity Framework SaveChanges() Throws Validation Errors

**Problem:**
```
An error occurred while updating the entries. See the inner exception for details.
```

or

```
The value 'string' is not valid for column 'Status'
```

**Root Cause:**

Entity Framework validation failed before saving to the database.

**Solutions:**

1. **Disable validation before SaveChanges():**
   ```csharp
   _db.Configuration.ValidateOnSaveEnabled = false;
   _db.SaveChanges();
   _db.Configuration.ValidateOnSaveEnabled = true;
   ```

2. **Check model annotations:**
   ```csharp
   [Required]
   [MaxLength(50)]
   public string Status { get; set; }
   ```

3. **Verify data before saving:**
   ```csharp
   if (!ModelState.IsValid)
   {
       var errors = ModelState.Values.SelectMany(v => v.Errors);
       return Json(new { IsError = true, ErrorMessage = string.Join("; ", errors) });
   }
   ```

---

### Issue 7: Entity Framework Query Performance Issues

**Problem:**
```
The query is slow and taking too long to return results.
```

**Solutions:**

1. **Use AsNoTracking() for read-only queries:**
   ```csharp
   IQueryable<Tickets> query = _db.Tickets.AsNoTracking();
   ```

2. **Avoid N+1 query problems:**
   ```csharp
   // Bad: Lazy loading in a loop
   foreach (var ticket in _db.Tickets)
   {
       var related = ticket.RelatedEntity;  // Triggers additional query per item
   }

   // Good: Eager loading
   var tickets = _db.Tickets.Include(t => t.RelatedEntity).ToList();
   ```

3. **Add database indexes:**
   ```csharp
   [Index("IX_Tickets_PublicTicketId", IsUnique = true)]
   public string PublicTicketId { get; set; }
   ```

4. **Use SQL Profiler to analyze queries:**
   - Enable `Database.Log` to see generated SQL.
   - Use SQL Server Profiler to trace slow queries.

---

### Issue 8: Syncfusion License Warning

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

You have successfully created an ASP.NET MVC5 application with Entity Framework 6 that includes:

✅ SQL Server database setup with the `Tickets` table  
✅ Entity Framework 6 for robust, feature-rich database operations  
✅ Change tracking for automatic entity state management  
✅ Fluent configuration in DbContext for entity mapping  
✅ Syncfusion EJ2 Grid with **UrlAdaptor** for standard CRUD operations  
✅ Syncfusion EJ2 Grid with **CustomAdaptor** for custom business logic  
✅ Full support for filtering, sorting, searching, paging, and batch operations  
✅ Error handling and troubleshooting guidelines specific to Entity Framework  
✅ SQL query logging for development and debugging  

### Next Steps

1. **Test the application** by running it in Visual Studio (F5).
2. **Verify CRUD operations** by adding, editing, and deleting records.
3. **Monitor SQL queries** using the Database.Log output to optimize performance.
4. **Customize the Grid columns** based on your specific business requirements.
5. **Implement custom filtering** in the CustomAdaptor for advanced scenarios.
6. **Deploy to production** following your organization's deployment procedures.

### When to Choose Entity Framework vs LINQ2DB

**Choose Entity Framework if your project:**
- Requires complex business logic with multiple related entities
- Needs automatic database migrations for schema management
- Demands change tracking and entity navigation
- Is a long-term project where maintainability is critical
- Has a large team familiar with EF patterns

**Choose LINQ2DB if your project:**
- Focuses on simple CRUD operations and data grid display
- Prioritizes performance and minimal configuration overhead
- Has a stable, predefined database schema
- Needs lightweight, fast ORM functionality
- Is a small to medium project with simple data access patterns

### Additional Resources

- [Syncfusion EJ2 ASP.NET MVC Grid Documentation](https://www.syncfusion.com/aspnet-mvc-components/mvc-grid)
- [Entity Framework 6 Official Documentation](https://learn.microsoft.com/en-us/ef/ef6/)
- [ASP.NET MVC 5 Official Documentation](https://docs.microsoft.com/en-us/aspnet/mvc/mvc5)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/sql-server/)

---

**Documentation Version:** 1.0  
**Last Updated:** February 1, 2026  
**Compatible with:** Syncfusion 32.1.19, ASP.NET MVC 5.3.0, Entity Framework 6.5.1, .NET Framework 4.8.1
