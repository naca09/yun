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





    }
}
