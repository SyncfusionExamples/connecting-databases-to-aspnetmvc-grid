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

            using (var db = new AppDataConnection())
            {
                // Retrieve data from the data source (e.g., database).
                // Base from raw SQL (IQueryable<Ticket> from FromSql)
                IQueryable<Tickets> query = db.FromSql<Tickets>(
                    @"SELECT * FROM dbo.Tickets"); // For Syncfusion built-in data operation handling methods usage purpose, required to return IQueryable<Tickets>, so using linq2db's FromSql to get IQueryable<Tickets> from raw SQL.
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

        // CREATE  ------------------------------------------------
        [HttpPost]
        public async Task<ActionResult> Insert(Tickets value)
        {
            // If DB sets CreatedAt/UpdatedAt via defaults/triggers, don’t touch them.
            // If you want app-driven timestamps instead, set them here and remove SkipOnInsert above.
            using (var db = new AppDataConnection())
            {
                // returns generated identity (int)
                var newId = await db.InsertWithInt32IdentityAsync(value);  // linq2db Insert API  [3](https://smarttechsavvy.com/how-do-i-enable-debugging-in-web-config/)
                value.TicketId = newId;
                return Json(value); // EJ2 expects the created row back
            }
        }

        // UPDATE  ------------------------------------------------
        // UrlAdaptor will post the edited entity. Primary key must be present.
        [HttpPost]
        public async Task<ActionResult> Update(Tickets value)
        {
            using (var db = new AppDataConnection())
            {
                // quick existence check
                bool exists = await db.Tickets.AnyAsync(t => t.TicketId == value.TicketId);
                if (!exists) return HttpNotFound("Record not found");

                // Option A: entity-based update by PK (requires [PrimaryKey] mapping)
                // Any column marked SkipOnUpdate will be ignored.
                var rows = await db.UpdateAsync(value);  // linq2db Update API  [3](https://smarttechsavvy.com/how-do-i-enable-debugging-in-web-config/)

                // Option B (fine-grained update, if you prefer explicit fields):
                // await db.Tickets
                //   .Where(t => t.TicketId == value.TicketId)
                //   .Set(t => t.PublicTicketId, value.PublicTicketId)
                //   .Set(t => t.Title,          value.Title)
                //   .Set(t => t.Description,    value.Description)
                //   .Set(t => t.Category,       value.Category)
                //   .Set(t => t.Department,     value.Department)
                //   .Set(t => t.Assignee,       value.Assignee)
                //   .Set(t => t.CreatedBy,      value.CreatedBy)
                //   .Set(t => t.Status,         value.Status)
                //   .Set(t => t.Priority,       value.Priority)
                //   .Set(t => t.ResponseDue,    value.ResponseDue)
                //   .Set(t => t.DueDate,        value.DueDate)
                //   //.Set(t => t.UpdatedAt,      Sql.CurrentTimestamp) // if you stamp on update
                //   .UpdateAsync();

                return Json(value);
            }
        }

        // Remove by CRUDModel<Tickets> (UrlAdaptor default)
        [HttpPost]
        public async Task<ActionResult> Remove(CRUDModel<Tickets> model)
        {
            var key = Convert.ToInt32(model.Key);
            using (var db = new AppDataConnection())
            {
                var rows = await db.Tickets.DeleteAsync(t => t.TicketId == key); // linq2db Delete API  [3](https://smarttechsavvy.com/how-do-i-enable-debugging-in-web-config/)
                if (rows == 0) return HttpNotFound("Record not found");
                return Json(model); // can also return { key } or deleted entity
            }
        }

        [HttpPost]
        public async Task<JsonResult> BatchUpdate(CRUDModel<Tickets> payload)
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
                        r.TicketId = newId; // send generated key back to Grid
                    }
                }

                // 2) UPDATE many (explicit SETs; full-row update shown—customize to changed fields if desired)
                if (payload.Changed != null && payload.Changed.Count > 0)
                {
                    foreach (var r in payload.Changed)
                    {
                        var rows = await db.Tickets
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

                        // optional: check if (rows==0) to detect missing keys
                    }
                }

                // 3) DELETE many (single WHERE IN)
                if (payload.Deleted != null && payload.Deleted.Count > 0)
                {
                    var keys = payload.Deleted.Select(d => d.TicketId).ToArray();
                    await db.Tickets.Where(t => keys.Contains(t.TicketId)).DeleteAsync();
                }

                await tr.CommitAsync();

                // Return the same payload (or a fresh read if you prefer)
                return Json(payload, JsonRequestBehavior.AllowGet);
            }
        }

    }
}