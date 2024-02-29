using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Please enter customer name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter customer address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please enter customer email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter customer phone number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }
    }
}
