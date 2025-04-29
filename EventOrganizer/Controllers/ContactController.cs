using EventOrganizer.Database;
using EventOrganizer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


namespace EventOrganizer.Controllers
{

        [Route("api/[controller]")]
        [ApiController]
        public class ContactController : ControllerBase
        {
            private readonly ApplicationDbContext _db;

            public ContactController(ApplicationDbContext db)
            {
                _db = db;
            }

            // List all contacts
            [HttpGet]
            [Route("GetAllList")]
            public async Task<IActionResult> GetAsync()
            {
                var contact = await _db.Contact.ToListAsync();
                return Ok(contact);
            }

            //GetById
            [HttpGet]
            [Route("GetUserById")]
            public async Task<IActionResult> GetAeroplaniByIdAsync(int Id)
            {
                var contact = await _db.Contact.FindAsync(Id);
                return Ok(contact);
            }

            //Add
            [HttpPost]
            [Route("Add")]
            public async Task<IActionResult> PostAsync(Contact contact)
            {
                _db.Contact.Add(contact);
                await _db.SaveChangesAsync();
                return Created($"/GetUserById/{contact.Id}", contact);
            }

            [Route("Delete")]
            [HttpDelete]
            public async Task<IActionResult> DeleteAsync(int Id)
            {
                var contactDelete = await _db.Contact.FindAsync(Id);
                if (contactDelete == null)
                {
                    return NotFound();
                }
                _db.Contact.Remove(contactDelete);
                await _db.SaveChangesAsync();
                return NoContent();
            }

        }
    }