using EFMVC5Grid.Models;
using Syncfusion.EJ2.Base;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace EFMVC5Grid.Controllers
{
    public class GridController : Controller
    {
        // Prefer per-request lifetime (controller instance) instead of static
        private readonly TicketsDbContext _db;

        public GridController()
        {
            _db = new TicketsDbContext(); // uses Web.config connection string by name
        }
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
        public ActionResult UrlDatasource(DataManagerRequest DataManagerRequest)
        {
            // Retrieve data from the data source (e.g., database).
            IQueryable<Tickets> query = _db.Tickets.AsNoTracking();
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
            else if (DataManagerRequest.Skip != null)
            {
                // EF6 needs an OrderBy before Skip, choose a stable identity key
                query = query.OrderBy(t => t.TicketId);
            }


            // Get the total records count.
            int count = query.Count();

            // Handling paging operation.
            if (DataManagerRequest.Skip != 0)
            {
                query = operation.PerformSkip(query, DataManagerRequest.Skip);
            }
            if (DataManagerRequest.Take != 0)
            {
                query = operation.PerformTake(query, DataManagerRequest.Take);
            }

            // Return data based on the request.
            return DataManagerRequest.RequiresCounts ? Json(new { result = query, count }) : Json(query);
        }

        /// <summary>
        /// Inserts a new data item into the data collection.
        /// </summary>
        /// <returns>Returns void.</returns>
        public ActionResult Insert(Tickets value)
        {
            _db.Tickets.Add(value);
            _db.SaveChanges();

            // Ensure returning the expected format.
            return Json(value);
        }

        /// <summary>
        /// Update a existing data item from the data collection.
        /// </summary>
        /// <returns>Returns void.</returns>
        [HttpPost]
        public ActionResult Update(Tickets value)
        {
            var exists = _db.Tickets.Any(t => t.TicketId == value.TicketId);
            if (!exists) return HttpNotFound("Record not found");

            // Update existing record.
            _db.Entry(value).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Json(value);
        }

        /// <summary>
        /// Remove a specific data item from the data collection.
        /// </summary>
        /// <return>Returns void.</return>
        [HttpPost]
        public ActionResult Remove(CRUDModel<Tickets> value)
        {
            var key = Convert.ToInt32(value.Key); // ensure key type matches your PK
            var entity = _db.Tickets.FirstOrDefault(t => t.TicketId == key);
            if (entity == null) return HttpNotFound("Record not found");

            // Remove the record from the data collection.
            _db.Tickets.Remove(entity);
            _db.SaveChanges();

            return Json(value);
        }

        /// <summary>
        /// Batch update (Insert, Update, and Delete) a collection of data items from the data collection.
        /// </summary>
        /// <param name="value">The set of information along with details about the CRUD actions to be executed from the database.</param>
        /// <returns>Returns a JsonResult containing the processed data.</returns>
        public JsonResult BatchUpdate(CRUDModel<Tickets> value)
        {
            // Handle changed records.
            if (value.Changed != null && value.Changed.Count > 0)
            {
                foreach (Tickets Record in value.Changed)
                {
                    // Update the changed records.
                    _db.Tickets.Attach(Record);
                    _db.Entry(Record).State = EntityState.Modified;
                }
            }

            // Handle added records.
            if (value.Added != null && value.Added.Count > 0)
            {
                foreach (Tickets order in value.Added)
                {
                    // This ensures EF does not try to insert TicketId.
                    order.TicketId = default;

                    // Add new records.
                    _db.Tickets.Add(order);
                }
            }

            // Handle deleted records.
            if (value.Deleted != null && value.Deleted.Count > 0)
            {
                foreach (Tickets Record in value.Deleted)
                {
                    // Find and delete the records.
                    var existingOrder = _db.Tickets.Find(Record.TicketId);
                    if (existingOrder != null)
                    {
                        _db.Tickets.Remove(existingOrder);
                    }
                }
            }

            // Save all changes to the database.
            _db.SaveChanges();
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Ensure DbContext is disposed with the controller.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }

    }
}