using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using EventOrganizer.Database;
using EventOrganizer.Models;

namespace EventOrganizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventThemesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public EventThemesController(ApplicationDbContext db)
        {
            _db = db;
        }

        //ListAll
        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAsync()
        {
            var themes = await _db.EventThemes.ToListAsync();
            return Ok(themes);
        }


        //GetById
        [HttpGet]
        [Route("GetEventThemesById")]
        public async Task<IActionResult> GetEventThemesByIdAsync(int Id)
        {
            var themes = await _db.EventThemes.FindAsync(Id);
            return Ok(themes);
        }


        //Add
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> PostAsync(EventThemes themes)
        {
            _db.EventThemes.Add(themes);
            await _db.SaveChangesAsync();
            return Created($"/GetEventThemesById/{themes.Id}", themes);
        }
    }
}