using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("classes")]
    public class ClassesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClassesController(AppDbContext context)
        {
            _context = context;
        }

        // ১. POST /classes : Create a new class 
        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request)
        {
            var newClass = new SchoolClass
            {
                Name = request.Name,
                TotalSeats = request.TotalSeats
            };

            _context.Classes.Add(newClass);
            await _context.SaveChangesAsync();

            return Created($"/classes/{newClass.Id}", new
            {
                id = newClass.Id,
                name = newClass.Name,
                totalSeats = newClass.TotalSeats
            });
        }

        // ২. GET /classes : all class list, seat-counts
        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            var currentTime = DateTime.UtcNow;

            var classes = await _context.Classes.ToListAsync();
            var classResponses = new List<ClassResponseDto>();

            foreach (var cls in classes)
            {
                // Counting unexpired HELD holds
                var heldSeats = await _context.Holds
                    .CountAsync(h => h.ClassId == cls.Id && h.Status == "HELD" && h.ExpiresAt > currentTime);

                var confirmedSeats = await _context.Holds
                    .CountAsync(h => h.ClassId == cls.Id && h.Status == "CONFIRMED");

                classResponses.Add(new ClassResponseDto
                {
                    Id = cls.Id,
                    Name = cls.Name,
                    TotalSeats = cls.TotalSeats,
                    HeldSeats = heldSeats,
                    ConfirmedSeats = confirmedSeats,
                    AvailableSeats = cls.TotalSeats - (heldSeats + confirmedSeats)
                });
            }

            return Ok(classResponses);
        }

        // ৩. GET /classes/{classId} : Seat status of a class
        [HttpGet("{classId}")]
        public async Task<IActionResult> GetClassStatus(string classId)
        {
            var cls = await _context.Classes.FindAsync(classId);
            if (cls == null) return NotFound(new { message = "Class not found" });

            var currentTime = DateTime.UtcNow;

            var heldSeats = await _context.Holds
                .CountAsync(h => h.ClassId == cls.Id && h.Status == "HELD" && h.ExpiresAt > currentTime);

            var confirmedSeats = await _context.Holds
                .CountAsync(h => h.ClassId == cls.Id && h.Status == "CONFIRMED");

            var response = new ClassResponseDto
            {
                Id = cls.Id,
                Name = cls.Name,
                TotalSeats = cls.TotalSeats,
                HeldSeats = heldSeats,
                ConfirmedSeats = confirmedSeats,
                AvailableSeats = cls.TotalSeats - (heldSeats + confirmedSeats)
            };

            return Ok(response);
        }
    }
}