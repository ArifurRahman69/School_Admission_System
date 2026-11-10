using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class SchoolClass
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString("N"); 

        [Required]
        public string Name { get; set; } = string.Empty; // example: "Class 6 - Morning Shift"

        [Required]
        public int TotalSeats { get; set; } // Total number of seats
    }
}