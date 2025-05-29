using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrganizingEvents.Models;
using OrganizingEvents.Data;
using Microsoft.EntityFrameworkCore;
using OrganizingEvents.Migrations;

namespace OrganizingEvents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public RolesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAsync()
        {
            var role = await _db.Roles.ToListAsync();
            return Ok(role);
        }

        [HttpGet]
        [Route("GetRoleById")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            return Ok(role);
        }

        [HttpPost]
        [Route("AddRole")]
        public async Task<IActionResult> PostAsync(Roles role)
        {
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return Created($"/GetRoleById/{role.Id}", role);
        }

        [HttpPut]
        [Route("UpdateRole")]
        public async Task<IActionResult> PutAsync(Roles role)
        {
            _db.Roles.Update(role);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var roleIdToDelete = await _db.Roles.FindAsync(id);
            if (roleIdToDelete == null)
            {
                return NotFound();
            }
            _db.Roles.Remove(roleIdToDelete);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

