namespace backend.DTOs
{
    // POST /classes 
    public class CreateClassRequest
    {
        public string Name { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
    }

    // GET /classes 
    public class ClassResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int HeldSeats { get; set; }
        public int ConfirmedSeats { get; set; }
        public int AvailableSeats { get; set; }
    }
}