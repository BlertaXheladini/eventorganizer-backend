using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using EventOrganizer.Database;
using EventOrganizer.Models;


namespace EventOrganizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public StaffController(ApplicationDbContext db)
        {
            _db = db;
        }

        //ListAll
        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAsync()
        {
            var staff = await _db.Staff.ToListAsync();
            return Ok(staff);
        }


        //GetById
        [HttpGet]
        [Route("GetStaffById")]
        public async Task<IActionResult> GetStaffByIdAsync(int Id)
        {
            var staff = await _db.Staff.FindAsync(Id);
            return Ok(staff);
        }


        //Add
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> PostAsync(Staff staff)
        {
            _db.Staff.Add(staff);
            await _db.SaveChangesAsync();
            return Created($"/GetStaffById/{staff.Id}", staff);
        }


        //Update
        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IActionResult> PutAsync(Staff staff)
        {
            _db.Staff.Update(staff);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        //Delete
        [Route("Delete")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int Id)
        {
            var staffIdDelete = await _db.Staff.FindAsync(Id);
            if (staffIdDelete == null)
            {
                return NotFound();
            }
            _db.Staff.Remove(staffIdDelete);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

