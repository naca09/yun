using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Category
    {
        [Key]
        public int categoryId {  get; set; }
        [Required]
        public string Name { get; set; }

        public string categoryImg { get; set; }
    }
}
