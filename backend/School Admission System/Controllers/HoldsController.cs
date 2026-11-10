using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.RegularExpressions;

namespace backend.Controllers
{
    [ApiController]
    public class HoldsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public HoldsController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Helper method to read the student ID from the header
        private string? GetStudentId()
        {
            if (Request.Headers.TryGetValue("X-Student-Id", out var studentId))
                return studentId.ToString();
            return null;
        }

        // 4. POST /classes/{classId}/holds : Apply (seat hold)
        [HttpPost("/classes/{classId}/holds")]
        public async Task<IActionResult> CreateHold(string classId)
        {
            var studentId = GetStudentId();

            // Regex validation: Must be exactly 17 digits (0-9)
            if (string.IsNullOrEmpty(studentId) || !Regex.IsMatch(studentId, @"^\d{17}$"))
                return BadRequest(new { message = "Valid 17-digit numeric X-Student-Id header is required" });

            // Concurrency locking: Transaction used to prevent overbooking from concurrent requests
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var cls = await _context.Classes.FindAsync(classId);
                if (cls == null) return NotFound(new { message = "Class not found" });

                var currentTime = DateTime.UtcNow;

                // Check: Does this student already have an active hold or confirmed seat in this class?
                var existingHold = await _context.Holds
                    .AnyAsync(h => h.ClassId == classId && h.StudentId == studentId &&
                                   (h.Status == "CONFIRMED" || (h.Status == "HELD" && h.ExpiresAt > currentTime)));

                if (existingHold)
                    return BadRequest(new { message = "Student already has an active hold or confirmed seat in this class" });

                // Calculate seats
                var heldSeats = await _context.Holds.CountAsync(h => h.ClassId == classId && h.Status == "HELD" && h.ExpiresAt > currentTime);
                var confirmedSeats = await _context.Holds.CountAsync(h => h.ClassId == classId && h.Status == "CONFIRMED");

                if (heldSeats + confirmedSeats >= cls.TotalSeats)
                    return Conflict(new { message = "Seat not available" }); // 409 Conflict if no seats are left

                // Get hold duration from Environment Variable, default to 600 seconds
                int holdDuration = _configuration.GetValue<int>("HOLD_DURATION_SECONDS", 600);

                var hold = new SeatHold
                {
                    ClassId = classId,
                    StudentId = studentId,
                    Status = "HELD",
                    ExpiresAt = currentTime.AddSeconds(holdDuration)
                };

                _context.Holds.Add(hold);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(201, new
                {
                    holdId = hold.HoldId,
                    classId = hold.ClassId,
                    studentId = hold.StudentId,
                    status = hold.Status,
                    expiresAt = hold.ExpiresAt
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Internal server error or concurrency conflict" });
            }
        }

        // 5. GET /holds/{holdId} : Current status of the hold
        [HttpGet("/holds/{holdId}")]
        public async Task<IActionResult> GetHoldStatus(string holdId)
        {
            var hold = await _context.Holds.FindAsync(holdId);
            if (hold == null) return NotFound();

            // Dynamic expiration check (without API call or background job)
            string currentStatus = hold.Status;
            if (hold.Status == "HELD" && hold.ExpiresAt <= DateTime.UtcNow)
            {
                currentStatus = "EXPIRED";
            }

            return Ok(new
            {
                holdId = hold.HoldId,
                classId = hold.ClassId,
                status = currentStatus,
                expiresAt = hold.ExpiresAt
            });
        }

        // 6. POST /holds/{holdId}/confirm : Submit admission fee and confirm
        [HttpPost("/holds/{holdId}/confirm")]
        public async Task<IActionResult> ConfirmHold(string holdId)
        {
            var studentId = GetStudentId();
            if (string.IsNullOrEmpty(studentId) || !Regex.IsMatch(studentId, @"^\d{17}$"))
                return BadRequest(new { message = "Valid 17-digit numeric X-Student-Id header is required" });

            var hold = await _context.Holds.FindAsync(holdId);
            if (hold == null) return NotFound();

            if (hold.StudentId != studentId) return Forbid(); // Can only confirm own hold

            // Idempotent: If already CONFIRMED, return success without doing anything
            if (hold.Status == "CONFIRMED")
                return Ok(new { holdId = hold.HoldId, status = hold.Status });

            if (hold.Status != "HELD" || hold.ExpiresAt <= DateTime.UtcNow)
                return BadRequest(new { message = "Hold is not active or has expired" });

            hold.Status = "CONFIRMED";
            await _context.SaveChangesAsync();

            return Ok(new { holdId = hold.HoldId, status = hold.Status });
        }

        // 7. DELETE /holds/{holdId} : Cancel hold
        [HttpDelete("/holds/{holdId}")]
        public async Task<IActionResult> CancelHold(string holdId)
        {
            var studentId = GetStudentId();
            if (string.IsNullOrEmpty(studentId) || !Regex.IsMatch(studentId, @"^\d{17}$"))
                return BadRequest(new { message = "Valid 17-digit numeric X-Student-Id header is required" });

            var hold = await _context.Holds.FindAsync(holdId);

            if (hold == null) return NoContent();
            if (hold.StudentId != studentId) return Forbid();

            hold.Status = "CANCELLED";
            await _context.SaveChangesAsync();

            return NoContent(); // 204 on success
        }

        // 8. GET /me/holds : All holds of the student
        [HttpGet("/me/holds")]
        public async Task<IActionResult> GetMyHolds()
        {
            var studentId = GetStudentId();
            if (string.IsNullOrEmpty(studentId) || !Regex.IsMatch(studentId, @"^\d{17}$"))
                return BadRequest(new { message = "Valid 17-digit numeric X-Student-Id header is required" });

            var currentTime = DateTime.UtcNow;

            var holds = await _context.Holds
                .Where(h => h.StudentId == studentId)
                .Join(_context.Classes, h => h.ClassId, c => c.Id, (h, c) => new
                {
                    holdId = h.HoldId,
                    classId = h.ClassId,
                    className = c.Name,
                    // Dynamic status update
                    status = (h.Status == "HELD" && h.ExpiresAt <= currentTime) ? "EXPIRED" : h.Status,
                    expiresAt = h.ExpiresAt
                })
                .ToListAsync();

            return Ok(holds);
        }
    }
}