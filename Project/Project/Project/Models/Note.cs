using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class Note
    {
        [Key]
        public int NoteId { get; set; } // Khóa chính
        [Required]

        [Display(Name = "Note Code")]
        public string NoteCode { get; set; } // Không null
        [Required]

        [StringLength(255)]
        [Display(Name = "Created's Name: ")]
        public string CreateName { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Customer: ")]
        public string Customer { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Customer's Addess: ")]
        public string AddressCustomer { get; set; }

        [Required]
        [Display(Name = "Reason: ")]
        public string Reason { get; set; }

        [Display(Name = "Date Created: ")]
        public DateTime CreatedDate { get; set; } // Auto-generated

        [Display(Name = "Total Bill: ")]
        public decimal Total { get; set; }


        [Range(1, 4, ErrorMessage = "Status must be between 1 and 4")]
        public int Status { get; set; } = 1; // Default value is 1

        public void UpdateStatus(int newStatus)
        {
            if (newStatus >= 1 && newStatus <= 4)
            {
                Status = newStatus;
            }
        }
        public virtual ICollection<NoteProduct> NoteProducts { get; set; }


    }
}
