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
    public class GridController : Controller
    {
        // GET: Grid
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Processes the DataManager request to perform searching, filtering, sorting, and paging operations.
        /// </summary>
        /// <param name="DataManagerRequest">Contains the details of the data operation requested.</param>
        /// <returns>Returns a JSON object with the filtered, sorted, and paginated data along with the total record count.</returns>
        [HttpPost]
        public async Task<ActionResult> UrlDatasource(DataManagerRequest DataManagerRequest)
        {
            try
            {
                using (var db = new AppDataConnection())
                {
                    // Retrieve data from the data source (e.g., database).
                    // Base from raw SQL (IQueryable<Ticket> from FromSql)
                    IQueryable<Transactions> query = db.FromSql<Transactions>(
                        @"SELECT * FROM transactiondb.transactions"); // For Syncfusion built-in data operation handling methods usage purpose, required to return IQueryable<Transactions>, so using linq2db's FromSql to get IQueryable<Transactions> from raw SQL.
                    QueryableOperation operation = new QueryableOperation(); // Initialize DataOperations instance.

                    // Handling Searching.
                    if (DataManagerRequest.Search != null && DataManagerRequest.Search.Count > 0)
                    {
                        query = operation.PerformSearching(query, DataManagerRequest.Search);
                    }

                    // Handling filtering operation.
                    if (DataManagerRequest.Where != null && DataManagerRequest.Where.Count > 0)
                    {
                        query = operation.PerformFiltering(query, DataManagerRequest.Where, DataManagerRequest.Where[0].Operator);
                    }

                    // Handling Sorting operation.
                    if (DataManagerRequest.Sorted != null && DataManagerRequest.Sorted.Count > 0)
                    {
                        query = operation.PerformSorting(query, DataManagerRequest.Sorted);
                    }

                    // Get the total records count.
                    int count = await query.CountAsync();

                    // Handling paging operation.
                    if (DataManagerRequest.Skip != 0)
                    {
                        query = operation.PerformSkip(query, DataManagerRequest.Skip);
                    }
                    if (DataManagerRequest.Take != 0)
                    {
                        query = operation.PerformTake(query, DataManagerRequest.Take);
                    }
                    var rows = await query.ToListAsync();
                    // Return data based on the request.
                    return DataManagerRequest.RequiresCounts ? Json(new { result = rows, count }) : Json(rows);
                }
            }
            catch (Exception ex)
            {
                // Temporarily show details while debugging
                return Content("ERROR: " + ex.ToString());
            }

        }

        // CREATE  ------------------------------------------------
        [HttpPost]
        public async Task<ActionResult> Insert(Transactions value)
        {
            // If DB sets CreatedAt/UpdatedAt via defaults/triggers, don’t touch them.
            // If you want app-driven timestamps instead, set them here and remove SkipOnInsert above.
            using (var db = new AppDataConnection())
            {
                // returns generated identity (int)
                var newId = await db.InsertWithInt32IdentityAsync(value);  // linq2db Insert API  [3](https://smarttechsavvy.com/how-do-i-enable-debugging-in-web-config/)
                value.Id = newId;
                return Json(value); // EJ2 expects the created row back
            }
        }

        // UPDATE  ------------------------------------------------
        // UrlAdaptor will post the edited entity. Primary key must be present.
        [HttpPost]
        public async Task<ActionResult> Update(Transactions value)
        {
            using (var db = new AppDataConnection())
            {
                // quick existence check
                bool exists = await db.Transactions.AnyAsync(t => t.Id == value.Id);
                if (!exists) return HttpNotFound("Record not found");

                // Option A: entity-based update by PK (requires [PrimaryKey] mapping)
                // Any column marked SkipOnUpdate will be ignored.
                var rows = await db.UpdateAsync(value);  // linq2db Update API  [3](https://smarttechsavvy.com/how-do-i-enable-debugging-in-web-config/)


                // Option B (fine-grained update, if you prefer explicit fields):
                // await db.Transactions
                //   .Where(t => t.Id == value.Id)
                //   .Set(t => t.TransactionId,   value.TransactionId)
                //   .Set(t => t.CustomerId,      value.CustomerId)
                //   .Set(t => t.OrderId,         value.OrderId)
                //   .Set(t => t.InvoiceNumber,   value.InvoiceNumber)
                //   .Set(t => t.Description,     value.Description)
                //   .Set(t => t.Amount,          value.Amount)
                //   .Set(t => t.CurrencyCode,    value.CurrencyCode)
                //   .Set(t => t.TransactionType, value.TransactionType)
                //   .Set(t => t.PaymentGateway,  value.PaymentGateway)
                //   .Set(t => t.CreatedAt,       value.CreatedAt)     // keep if you intentionally allow updating CreatedAt
                //   .Set(t => t.CompletedAt,     value.CompletedAt)
                //   .Set(t => t.Status,          value.Status)
                //   //.Set(t => t.UpdatedAt,      Sql.CurrentTimestamp) // if you add an UpdatedAt column later
                //   .UpdateAsync();


                return Json(value);
            }
        }

        // Remove by CRUDModel<Transactions> (UrlAdaptor default)
        [HttpPost]
        public async Task<ActionResult> Remove(CRUDModel<Transactions> model)
        {
            var key = Convert.ToInt32(model.Key);
            using (var db = new AppDataConnection())
            {
                var rows = await db.Transactions.DeleteAsync(t => t.Id == key); // linq2db Delete API  [3](https://smarttechsavvy.com/how-do-i-enable-debugging-in-web-config/)
                if (rows == 0) return HttpNotFound("Record not found");
                return Json(model); // can also return { key } or deleted entity
            }
        }

        [HttpPost]
        public async Task<JsonResult> BatchUpdate(CRUDModel<Transactions> payload)
        {
            using (var db = new AppDataConnection())
            {
                var tr = await db.BeginTransactionAsync();

                // 1) INSERT many (return identities to client)
                if (payload.Added != null && payload.Added.Count > 0)
                {
                    foreach (var r in payload.Added)
                    {
                        // Insert only writable columns (skip PK/computed)
                        var newId = await db.InsertWithInt32IdentityAsync(r);
                        r.Id = newId; // send generated key back to Grid
                    }
                }

                // 2) UPDATE many (explicit SETs; full-row update shown—customize to changed fields if desired)
                if (payload.Changed != null && payload.Changed.Count > 0)
                {
                    foreach (var r in payload.Changed)
                    {

                        var rows = await db.Transactions
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
                            .Set(t => t.CreatedAt, r.CreatedAt)     // keep if you intentionally allow updating CreatedAt
                            .Set(t => t.CompletedAt, r.CompletedAt)
                            .Set(t => t.Status, r.Status)
                            .UpdateAsync();

                        // optional: check if (rows==0) to detect missing keys
                    }
                }

                // 3) DELETE many (single WHERE IN)
                if (payload.Deleted != null && payload.Deleted.Count > 0)
                {
                    var keys = payload.Deleted.Select(d => d.Id).ToArray();
                    await db.Transactions.Where(t => keys.Contains(t.Id)).DeleteAsync();
                }

                await tr.CommitAsync();

                // Return the same payload (or a fresh read if you prefer)
                return Json(payload, JsonRequestBehavior.AllowGet);
            }
        }
    }
}