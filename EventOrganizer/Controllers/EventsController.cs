using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using EventOrganizer.Models;
using EventOrganizer.Database;
using ClosedXML.Excel;
using System.IO;

namespace OrganizingEvents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public EventsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ListAll
        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAsync()
        {
            var events = await _db.Events
                .Include(q => q.EventThemes)
                .Include(q => q.EventCategories)
                .ToListAsync();
            return Ok(events);
        }

        // GetById
        [HttpGet("GetEventById/{id}")]
        public async Task<IActionResult> GetEventsByIdAsync(int id)
        {
            var events = await _db.Events
                .Include(q => q.EventThemes)
                .Include(q => q.EventCategories)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (events == null)
            {
                return NotFound();
            }
            return Ok(events);
        }

        // Add
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> PostAsync(Events events)
        {
            // Check if EventThemes exists
            var existingEventThemes = await _db.EventThemes.FindAsync(events.ThemeId);
            if (existingEventThemes == null)
            {
                return NotFound($"EventThemes with ID {events.ThemeId} does not exist.");
            }

            // Check if EventCategories exists
            var existingEventCategories = await _db.EventCategories.FindAsync(events.CategoryId);
            if (existingEventCategories == null)
            {
                return NotFound($"EventCategories with ID {events.CategoryId} does not exist.");
            }

            events.EventThemes = existingEventThemes;
            events.EventCategories = existingEventCategories;

            _db.Events.Add(events);
            await _db.SaveChangesAsync();
            return Created($"/GetEventsById/{events.Id}", events);
        }

        // Update
        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IActionResult> PutAsync(Events events)
        {
            // Check if EventThemes exists
            var existingEventThemes = await _db.EventThemes.FindAsync(events.ThemeId);
            if (existingEventThemes == null)
            {
                return NotFound($"EventThemes with ID {events.ThemeId} does not exist.");
            }

            // Check if EventCategories exists
            var existingEventCategories = await _db.EventCategories.FindAsync(events.CategoryId);
            if (existingEventCategories == null)
            {
                return NotFound($"EventCategories with ID {events.CategoryId} does not exist.");
            }

            events.EventThemes = existingEventThemes;
            events.EventCategories = existingEventCategories;

            _db.Events.Update(events);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Delete
        [Route("Delete")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int Id)
        {
            var eventsIdDelete = await _db.Events
                .Include(q => q.EventThemes)
                .Include(q => q.EventCategories)
                .FirstOrDefaultAsync(q => q.Id == Id);

            if (eventsIdDelete == null)
            {
                return NotFound();
            }

            _db.Events.Remove(eventsIdDelete);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Search Events by Name
        [HttpGet("SearchEvent")]
        public async Task<IActionResult> SearchEventAsync([FromQuery] string searchEvent)
        {
            if (string.IsNullOrEmpty(searchEvent))
            {
                return BadRequest("Search term cannot be empty.");
            }

            var events = await _db.Events
                .Include(e => e.EventThemes)
                .Include(e => e.EventCategories)
                .Where(e => EF.Functions.Like(e.EventName, $"%{searchEvent}%"))
                .ToListAsync();

            return Ok(events);
        }

        // Order Events by Price
        [HttpGet("SortEvents")]
        public async Task<IActionResult> SortEvents(string sortOrder)
        {
            var events = await _db.Events.ToListAsync();

            if (sortOrder == "LowToHigh")
            {
                events = events.OrderBy(e => e.Price).ToList();
            }
            else if (sortOrder == "HighToLow")
            {
                events = events.OrderByDescending(e => e.Price).ToList();
            }

            return Ok(events);
        }

        // Export Events to Excel
        [HttpGet]
        [Route("ExportEventsToExcel")]
        public async Task<IActionResult> ExportEventsToExcel()
        {
            // Fetch all events along with their themes and categories
            var events = await _db.Events
                .Include(e => e.EventThemes)
                .Include(e => e.EventCategories)
                .ToListAsync();

            // Prepare data for export
            var eventsForExport = events.Select(e => new
            {
                EventId = e.Id,
                EventName = e.EventName,
                Description = e.Description,
                Image = e.Image,
                Price = e.Price,
                EventTheme = e.EventThemes?.ThemeName,
                EventCategory = e.EventCategories?.CategoryName,
            }).ToList();

            // Create Excel file using ClosedXML
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Events");

            worksheet.Cell(1, 1).Value = "EventId";
            worksheet.Cell(1, 2).Value = "EventName";
            worksheet.Cell(1, 3).Value = "Description";
            worksheet.Cell(1, 4).Value = "Image";
            worksheet.Cell(1, 5).Value = "Price";
            worksheet.Cell(1, 6).Value = "EventTheme";
            worksheet.Cell(1, 7).Value = "EventCategory";

            int row = 2;
            foreach (var ev in eventsForExport)
            {
                worksheet.Cell(row, 1).Value = ev.EventId;
                worksheet.Cell(row, 2).Value = ev.EventName;
                worksheet.Cell(row, 3).Value = ev.Description;
                worksheet.Cell(row, 4).Value = ev.Image;
                worksheet.Cell(row, 5).Value = ev.Price;
                worksheet.Cell(row, 6).Value = ev.EventTheme;
                worksheet.Cell(row, 7).Value = ev.EventCategory;
                row++;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Events.xlsx");
            }
        }
    }
}
