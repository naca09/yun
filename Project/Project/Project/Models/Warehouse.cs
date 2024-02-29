using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Project.Models;
namespace Project.Models
{
    public class Warehouse
    {

        [Key]
        public int WarehouseId { get; set; }
/*        [Display = "name"]*/
        public string WarehouseName { get; set; }
        public string Address { get; set; }

        public int ZipCode { get; set; }
        public int PhoneNumber { get; set; }
    }
}
