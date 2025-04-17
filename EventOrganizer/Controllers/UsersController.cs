using EventOrganizer.Database;
using EventOrganizer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace EventOrganizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAsync()
        {
            var users = await _db.User.ToListAsync();
            return Ok(users);
        }

        [HttpGet]
        [Route("GetUserById")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _db.User.FindAsync(id);
            return Ok(user);
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> PostAsync(User user)
        {
            _db.User.Add(user);
            await _db.SaveChangesAsync();
            return Created($"/GetUserById/{user.Id}", user);
        }

        [HttpPut]
        [Route("UpdateUser")]
        public async Task<IActionResult> PutAsync(User user)
        {
            _db.User.Update(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var userIdToDelete = await _db.User.FindAsync(id);
            if (userIdToDelete == null)
            {
                return NotFound();
            }
            _db.User.Remove(userIdToDelete);
            await _db.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(User objUser)
        {
            var dbuser = _db.User.Where(u => u.Email == objUser.Email).FirstOrDefault();
            if (dbuser != null)
            {
                return BadRequest("Emaili ekziston");
            }

            var exisstingState = await _db.Roles.FindAsync(objUser.RoleId);
            if (exisstingState == null)
            {
                return NotFound($"Roli me ID {objUser.Role.Id} nuk ekziston");
            }

            objUser.Role = exisstingState;
            objUser.RefreshTokenExpiryTime = objUser.RefreshTokenExpiryTime ?? DateTime.UtcNow;

            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(objUser.Password, salt);

            objUser.Password = hashedPassword;
            _db.User.Add(objUser);
            await _db.SaveChangesAsync();
            return Ok("Regjistrimi u shtua me sukses.");
        }

    }
}