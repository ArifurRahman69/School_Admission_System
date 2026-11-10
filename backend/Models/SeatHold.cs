using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class SeatHold
    {
        [Key]
        public string HoldId { get; set; } = Guid.NewGuid().ToString("N"); // Unique ID for the application

        [Required]
        public string ClassId { get; set; } = string.Empty; // The class applied for

        [Required]
        [StringLength(17, MinimumLength = 17)]
        [RegularExpression(@"^\d{17}$", ErrorMessage = "Student ID must be exactly 17 numeric digits.")]
        public string StudentId { get; set; } = string.Empty; // 17-digit student birth registration number

        [Required]
        public string Status { get; set; } = "HELD"; // Can be HELD, CONFIRMED, EXPIRED, CANCELLED

        [Required]
        public DateTime ExpiresAt { get; set; } // Time when the hold expires
    }
}