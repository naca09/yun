using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rolepp.Controllers
{
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notes

        public async Task<IActionResult> Index(string sortOrder, int? pageNumber, string searchString)
        {
            IQueryable<Note> notes = _context.Notes
                .Include(n => n.NoteProducts)
                .ThenInclude(np => np.Product);

            switch (sortOrder)
            {
                case "newest":
                    notes = notes.OrderByDescending(s => s.CreatedDate);
                    break;
                case "oldest":
                    notes = notes.OrderBy(s => s.CreatedDate);
                    break;
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                notes = notes.Where(s => s.NoteCode.Contains(searchString));
            }

            int pageSize = 10;
            return View(await PaginatedList<Note>.CreateAsync(notes.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        // GET: Notes/Create
        public IActionResult Create()
        {
            // Retrieve products for dropdown
            var products = _context.Products.ToList();

            // Pass products to the ViewBag
            ViewBag.Products = products;

            return View();
        }

        // POST: Notes/Create
        // Trong phần [HttpPost] Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NoteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra tính hợp lệ của dữ liệu số
            foreach (var productViewModel in model.Products)
            {
                if (productViewModel.StockOut <= 0)
                {
                    ModelState.AddModelError("", "Stock out quantity must be a positive number.");
                    return View(model);
                }
            }

            try
            {
                decimal total = 0;

                // Lấy thông tin chi tiết của các sản phẩm một lần duy nhất
                var productIds = model.Products.Select(p => p.ProductID).ToList();
                var productsInDb = _context.Products.Where(p => productIds.Contains(p.ProductID)).ToList();

                foreach (var productViewModel in model.Products)
                {
                    var productInDb = productsInDb.FirstOrDefault(p => p.ProductID == productViewModel.ProductID);

                    if (productInDb == null)
                    {
                        ModelState.AddModelError("", "Invalid product selection.");
                        return View(model);
                    }

                    total += productInDb.Price * productViewModel.StockOut;
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    var note = new Note
                    {
                        NoteCode = model.NoteCode,
                        CreateName = model.CreateName,
                        Customer = model.Customer,
                        AddressCustomer = model.AddressCustomer,
                        Reason = model.Reason,
                        Status = 1,
                        CreatedDate = DateTime.UtcNow,
                        Total = total
                    };

                    _context.Notes.Add(note);
                    _context.SaveChanges();

                    foreach (var productViewModel in model.Products)
                    {
                        var noteProduct = new NoteProduct
                        {
                            NoteId = note.NoteId,
                            ProductID = productViewModel.ProductID,
                            StockOut = productViewModel.StockOut
                        };

                        _context.NoteProducts.Add(noteProduct);

                        // Giảm số lượng sản phẩm trong kho
                        var productInDb = productsInDb.FirstOrDefault(p => p.ProductID == productViewModel.ProductID);
                        if (productInDb != null)
                        {
                            productInDb.Quantity -= productViewModel.StockOut;
                        }
                    }

                    _context.SaveChanges();
                    transaction.Commit();

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the note. Please try again.");
                return View(model);
            }
        }




        // GET: Notes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            return View(note);
        }

        // POST: Notes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NoteId,NoteCode,CreateName,Customer,AddressCustomer,Reason,Status")] Note note)
        {
            if (id != note.NoteId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(note);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteExists(note.NoteId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        // GET: Notes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .Include(n => n.NoteProducts)
                .ThenInclude(np => np.Product)
                .FirstOrDefaultAsync(m => m.NoteId == id);

            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }
    

    // POST: Notes/Delete/5
    [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatusAjax(int id, int newStatus)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            note.UpdateStatus(newStatus);

            _context.Entry(note).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.NoteId == id);
        }
        public IActionResult GetNewNoteCount()
        {
            int newNoteCount = _context.Notes.Count(n => n.Status == 2);
            return Json(newNoteCount);
        }

        public IActionResult CheckNoteStatus()
        {
            bool hasNoteStatus34 = _context.Notes.Any(n => n.Status == 3 || n.Status == 4);
            return Json(hasNoteStatus34);
        }

 public IActionResult DownloadNoteDetails(int id)
 {
     var note = _context.Notes.Include(n => n.NoteProducts).ThenInclude(np => np.Product).FirstOrDefault(n => n.NoteId == id);
     if (note == null)
     {
         return NotFound();
     }
     ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

     using (var package = new ExcelPackage())
     {
         ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
         var worksheet = package.Workbook.Worksheets.Add("Note Details");

         // Add Note details
         AddValueWithBorder(worksheet, 1, 3, "Note Delivery Goods Information");
         AddValueWithBorder(worksheet, 2, 1, "Note Code:");
         AddValueWithBorder(worksheet, 2, 2, note.NoteCode);

         AddValueWithBorder(worksheet, 3, 1, "Created's Name:");
         AddValueWithBorder(worksheet, 3, 2, note.CreateName);

         AddValueWithBorder(worksheet, 4, 1, "Customer:");
         AddValueWithBorder(worksheet, 4, 2, note.Customer);

         AddValueWithBorder(worksheet, 5, 1, "Customer's Address:");
         AddValueWithBorder(worksheet, 5, 2, note.AddressCustomer);

         AddValueWithBorder(worksheet, 6, 1, "Reason:");
         AddValueWithBorder(worksheet, 6, 2, note.Reason);

         AddValueWithBorder(worksheet, 7, 1, "Date Created");
         AddValueWithBorder(worksheet, 7, 2, note.CreatedDate.ToString());
         AddValueWithBorder(worksheet, 8, 1, "Status:");
         AddValueWithBorder(worksheet, 8, 2, ConvertStatus(note.Status));
         AddValueWithBorder(worksheet, 10, 3, "Product to Export");

         // Add header for Products
         AddValueWithBorder(worksheet, 11, 1, "Product Name");
         AddValueWithBorder(worksheet, 11, 2, "Product Code");
         AddValueWithBorder(worksheet, 11, 3, "StockOut");
         AddValueWithBorder(worksheet, 11, 4, "Price");
         AddValueWithBorder(worksheet, 11, 5, "Total");

         // Add data for Products
         int row = 12;
         foreach (var product in note.NoteProducts)
         {
             AddValueWithBorder(worksheet, row, 1, product.Product.ProductName);
             AddValueWithBorder(worksheet, row, 2, product.Product.ProductCode);
             AddValueWithBorder(worksheet, row, 3, product.StockOut);
             AddValueWithBorder(worksheet, row, 4, product.Product.Price);
             AddValueWithBorder(worksheet, row, 5, product.StockOut * product.Product.Price);
             row++;
         }

         // Add total of Note
         AddValueWithBorder(worksheet, row, 4, "Total of Note:");
         AddValueWithBorder(worksheet, row, 5, note.NoteProducts.Sum(p => p.StockOut * p.Product.Price));

         // Save the Excel package to a MemoryStream
         var stream = new MemoryStream();
         package.SaveAs(stream);

         // Return the Excel file as a FileContentResult
         return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Note Code: " + note.NoteCode + ".xlsx");
     }
 }
 public IActionResult DownloadSearchResults()
 {
     DateTime fromDate = DateTime.Parse(TempData["FromDate"].ToString());
     DateTime toDate = DateTime.Parse(TempData["ToDate"].ToString());

     IQueryable<Note> notes = _context.Notes.Include(n => n.NoteProducts).ThenInclude(np => np.Product);

     notes = notes.Where(s => s.CreatedDate.Date >= fromDate.Date && s.CreatedDate.Date <= toDate.Date);
     ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

     using (var package = new ExcelPackage())
     {
         ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
         var worksheet = package.Workbook.Worksheets.Add("Search Results");

         // Add header
         AddValueWithBorder(worksheet, 1, 1, "Note Code");
         AddValueWithBorder(worksheet, 1, 2, "Created's Name");
         AddValueWithBorder(worksheet, 1, 3, "Customer");
         AddValueWithBorder(worksheet, 1, 4, "Customer's Address");
         AddValueWithBorder(worksheet, 1, 5, "Reason");
         AddValueWithBorder(worksheet, 1, 6, "Date Created");
         AddValueWithBorder(worksheet, 1, 7, "Products and StockOut"); // New column
         AddValueWithBorder(worksheet, 1, 8, "Total");
         AddValueWithBorder(worksheet, 1, 9, "Status");

         // Add data
         int row = 2;
         foreach (var note in notes)
         {
             AddValueWithBorder(worksheet, row, 1, note.NoteCode);
             AddValueWithBorder(worksheet, row, 2, note.CreateName);
             AddValueWithBorder(worksheet, row, 3, note.Customer);
             AddValueWithBorder(worksheet, row, 4, note.AddressCustomer);
             AddValueWithBorder(worksheet, row, 5, note.Reason);
             AddValueWithBorder(worksheet, row, 6, note.CreatedDate.ToString());

             // Create a string containing all products and their StockOut, separated by newlines
             var productsAndStockOut = string.Join("\n", note.NoteProducts.Select(np => np.Product.ProductName + ": " + np.StockOut));
             AddValueWithBorder(worksheet, row, 7, productsAndStockOut);

             AddValueWithBorder(worksheet, row, 8, note.NoteProducts.Sum(p => p.StockOut * p.Product.Price));
             AddValueWithBorder(worksheet, row, 9, ConvertStatus(note.Status));

             row++;
         }

         // Save the Excel package to a MemoryStream
         var stream = new MemoryStream();
         package.SaveAs(stream);

         // Return the Excel file as a FileContentResult
         return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SearchResults.xlsx");
     }
 }
 private void AddValueWithBorder(ExcelWorksheet worksheet, int row, int column, object value)
 {
     var cell = worksheet.Cells[row, column];
     cell.Value = value;
     cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
     cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
     cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
     cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
 }
 private string ConvertStatus(int status)
 {
     switch (status)
     {
         case 1:
         case 2:
             return "Waiting..";
         case 3:
             return "Approved";
         case 4:
             return "Disapproved";
         default:
             return "Unknown status";
     }
 }
 public ActionResult PrintNoteDetails(int id)
 {
     var note = _context.Notes.Find(id);
     if (note == null)
     {
         return NotFound();
     }

     return PartialView("_NoteDetails", note);
 }




    }
}
