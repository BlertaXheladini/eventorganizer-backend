using EventOrganizer.Database;
using EventOrganizer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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

        //ListAll
        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAsync()
        {
            var feedback = await _db.Feedback
                .Include(q => q.User)
                .Include(q => q.Events)
                .ToListAsync();
            return Ok(feedback);
        }


        //GetById
        [HttpGet]
        [Route("GetFeedBackById")]
        public async Task<IActionResult> GetFeedBackByIdAsync(int Id)
        {
            var feedback = await _db.Feedback
                .Include(q => q.User)
                .Include(q => q.Events)
                .FirstOrDefaultAsync(q => q.Id == Id);
            return Ok(feedback);
        }


        //Add
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> PostAsync(Feedback feedback)
        {
            // Kontrollo nëse User ekziston
            var existingUser = await _db.User.FindAsync(feedback.UserId);
            if (existingUser == null)
            {
                return NotFound($"User me ID {feedback.UserId} nuk ekziston.");
            }

            // Kontrollo nëse Events ekziston
            var existingEvents = await _db.Events.FindAsync(feedback.EventsId);
            if (existingEvents == null)
            {
                return NotFound($"Eventi me ID {feedback.EventsId} nuk ekziston.");
            }

            feedback.User = existingUser;
            feedback.Events = existingEvents;

            _db.Feedback.Add(feedback);
            await _db.SaveChangesAsync();
            return Created($"/GetFeedBackById/{feedback.Id}", feedback);
        }

        //Delete
        [Route("Delete")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int Id)
        {
            var feedbackIdDelete = await _db.Feedback
                .Include(q => q.User)
                .Include(q => q.Events)
                .FirstOrDefaultAsync(q => q.Id == Id);

            if (feedbackIdDelete == null)
            {
                return NotFound();
            }

            _db.Feedback.Remove(feedbackIdDelete);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
