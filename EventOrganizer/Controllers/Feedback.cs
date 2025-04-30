using EventOrganizer.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventOrganizer.Models;

namespace EventOrganizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public FeedbackController(ApplicationDbContext db)
        {
            _db = db;
        }


        [HttpGet("GetAllList")]
        public async Task<IActionResult> GetAsync()
        {
            var feedback = await _db.Feedback
                .Include(q => q.Events)
                .ThenInclude(e => e.EventCategories)
                .Include(q => q.Events)
                .ThenInclude(e => e.EventThemes)
                .ToListAsync();

            return Ok(feedback);
        }


        [HttpGet("GetFeedBackById")]
        public async Task<IActionResult> GetFeedBackByIdAsync(int id)
        {
            var feedback = await _db.Feedback
                .Include(q => q.Events)
                .ThenInclude(e => e.EventCategories)
                .Include(q => q.Events)
                .ThenInclude(e => e.EventThemes)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (feedback == null)
            {
                return NotFound($"Feedback me ID {id} nuk ekziston.");
            }

            return Ok(feedback);
        }


        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> PostAsync(Feedback feedback)
        {

            var existingEvent = await _db.Events
                .Include(e => e.EventCategories)
                .Include(e => e.EventThemes)
                .FirstOrDefaultAsync(e => e.Id == feedback.EventsId);

            if (existingEvent == null)
            {
                return NotFound($"Eventi me ID {feedback.EventsId} nuk ekziston.");
            }

            feedback.Events = existingEvent;

            _db.Feedback.Add(feedback);
            await _db.SaveChangesAsync();

            return Created($"/api/Feedback/GetFeedBackById/{feedback.Id}", feedback);
        }


        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var feedbackToDelete = await _db.Feedback
                .Include(q => q.Events)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (feedbackToDelete == null)
            {
                return NotFound();
            }

            _db.Feedback.Remove(feedbackToDelete);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}