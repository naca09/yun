using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class NoteProduct
    {
        [Key]
        public int NoteProductId { get; set; } // Khóa chính

        public int NoteId { get; set; } // Foreign key referencing Note

        public int ProductID { get; set; } // Foreign key referencing Product

        public int StockOut { get; set; }

        public virtual Note Note { get; set; } // Navigation property to parent Note
        public virtual Product Product { get; set; } // Navigation property to related Product




    }
}