using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Project.Data;
using Project.Models;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
namespace Project.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Product
        public async Task<IActionResult> Index(string searchString, int? pageNumber, bool lowQuantity = false, bool outOfStock = false)
        {
            var products = _context.Products.Include(p => p.Category).Include(p => p.Warehouse).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.ProductName.Contains(searchString));
            }

            if (lowQuantity)
            {
                products = products.Where(p => p.Quantity < 10);
            }
            if (outOfStock)
            {
                products = products.Where(p => p.InStock == false);
            }
            else
            {

                products = products.Where(p => p.InStock);
            }
            var inStockProducts = _context.Products.Where(p => p.InStock).ToList();

            // Thêm sản phẩm có trong kho vào ViewBag
            ViewBag.Products = new SelectList(inStockProducts, "ProductID", "ProductName");

            //mỗi product chiếm 2.5, 10 product sẽ chiếm 25 
            int pageSize = 25;
            return View(await PaginatedList<Product>.CreateAsync(products.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            ViewBag.Warehouses = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseName");
            ViewBag.Categories = new SelectList(_context.Category, "categoryId", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,Price,Quantity,ProductCode,WarehouseId,CategoryId")] Product product, [FromForm(Name = "file")] Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (product.ProductID != null && product.ProductName != null && product.Price != null && product.Quantity != null && product.ProductCode != null && product.WarehouseId != null && product.Warehouse == null && product.Category == null && product.CategoryId != null && product.ProductImg == null)
            {
                if (file != null && file.Length > 0)
                {
                    // Lưu tập tin vào thư mục images trong wwwroot
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    // Lưu thông tin hình ảnh vào trường ProductImg của Product model
                    product.ProductImg = uniqueFileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Warehouses = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseName", product.WarehouseId);
            ViewBag.Categories = new SelectList(_context.Category, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Warehouse) // Include Warehouse information
                .Include(p => p.Category) // Include Category information
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Warehouses = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseName", product.WarehouseId);
            ViewBag.Categories = new SelectList(_context.Category, "categoryId", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Product/Edit/5
        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,ProductName,Price,Quantity,ProductCode,WarehouseId,CategoryId")] Product product, [FromForm(Name = "file")] IFormFile file)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            
                if (product.ProductID != null && product.ProductName != null && product.Price != null && product.Quantity != null && product.ProductCode != null && product.WarehouseId != null && product.Warehouse == null && product.Category == null && product.CategoryId != null)
                {
                    try
                    {
                        if (file != null && file.Length > 0)
                        {
                            // Save the file to the 'img' folder in wwwroot
                            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            // Update the ProductImg field with the new file name
                            product.ProductImg = uniqueFileName;
                        }

                        _context.Update(product);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProductExists(product.ProductID))
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

            ViewBag.Warehouses = new SelectList(_context.Warehouses, "WarehouseId", "WarehouseName", product.WarehouseId);
            ViewBag.Categories = new SelectList(_context.Category, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }


        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Warehouse)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Check if there are any related records in NoteProducts
            var relatedNotes = await _context.NoteProducts.AnyAsync(np => np.ProductID == id);
            if (relatedNotes)
            {
                // Handle the situation where related records exist (e.g., display a message)
                // You can choose to delete related records first, or prompt the user, etc.
                // For example, you could return a view indicating that the product has related notes and prompting the user to confirm deletion.
                return View("DeleteError", product); // Create a view named DeleteError.cshtml to handle this scenario
            }

            // If there are no related records, proceed with deletion
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }




        public IActionResult Search(string searchText)
        {
            // Logic để tìm kiếm sản phẩm dựa trên searchText
            var products = _context.Products.Where(p => p.ProductName.Contains(searchText)).ToList();
            return View("Index", products);
        }
        [HttpGet]
        public JsonResult GetProductDetails(int id)
        {
            var product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id);
            if (product != null)
            {
                return Json(new { productCode = product.ProductCode, quantity = product.Quantity, price = product.Price, category = product.Category.Name });
            }
            return Json(null);
        }



        [HttpGet]
        public IActionResult ProductCount()
        {
            var lowQuantityProductCount = _context.Products.Count(p => p.Quantity < 10);
            return Json(lowQuantityProductCount);
        }
        public async Task<IActionResult> Download()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Products");
                AddValueWithBorder(worksheet, 1, 1, "Product Name");
                AddValueWithBorder(worksheet, 1, 2, "Price");
                AddValueWithBorder(worksheet, 1, 3, "Quantity");
                AddValueWithBorder(worksheet, 1, 4, "Product Code");
                AddValueWithBorder(worksheet, 1, 5, "Warehouse");
                AddValueWithBorder(worksheet, 1, 6, "Category"); // Add Category header
                var products = await _context.Products.Include(p => p.Warehouse).Include(p => p.Category).ToListAsync(); // Include Category
                for (int i = 0; i < products.Count; i++)
                {
                    AddValueWithBorder(worksheet, i + 2, 1, products[i].ProductName);
                    AddValueWithBorder(worksheet, i + 2, 2, products[i].Price);
                    AddValueWithBorder(worksheet, i + 2, 3, products[i].Quantity);
                    AddValueWithBorder(worksheet, i + 2, 4, products[i].ProductCode);
                    AddValueWithBorder(worksheet, i + 2, 5, products[i].Warehouse.WarehouseName); // Use WarehouseName
                    AddValueWithBorder(worksheet, i + 2, 6, products[i].Category.Name); // Use CategoryName
                }

                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
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

        public async Task<IActionResult> OutStock(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            product.InStock = false;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Ready(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            product.InStock = true;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMultiple(int[] productIds, bool inStock)
        {
            var products = await _context.Products.Where(p => productIds.Contains(p.ProductID)).ToListAsync();
            if (products == null)
            {
                return NotFound();
            }
            foreach (var product in products)
            {
                product.InStock = inStock;
                _context.Products.Update(product);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
