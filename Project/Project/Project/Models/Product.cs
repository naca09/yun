using Microsoft.EntityFrameworkCore;
using Project.Data.Migrations;
using Project.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class Product
    {
        public Product()
        {
            InStock = true;
        }
        [Key]
        public int ProductID { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [StringLength(20)]
        public string ProductCode { get; set; }

        [Required]
        public string ProductImg { get; set; }
        // Thêm categoryId vào Product
        public int? CategoryId { get; set; }

        public bool InStock {  get; set; }
        // Khai báo khóa ngoại Category
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        // Change to WarehouseId to match the foreign key convention
        public int? WarehouseId { get; set; }


        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; }
        public ICollection<NoteProduct> NoteProducts { get; set; }

    }
}
