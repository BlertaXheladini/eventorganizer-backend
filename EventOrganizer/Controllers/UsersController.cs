using EventOrganizer.Database;
using EventOrganizer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Azure.Core;
using Microsoft.IdentityModel.Tokens;


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

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(Login user)
        {
            var userInDb = await _db.User.SingleOrDefaultAsync(u => u.Email == user.Email);
            if (userInDb == null || !BCrypt.Net.BCrypt.Verify(user.Password, userInDb.Password))
            {
                return BadRequest("Emaili ose Fjalekalimi gabim.");
            }

            var tokens = GenerateTokens(userInDb);
            userInDb.RefreshToken = tokens.RefreshToken;
            userInDb.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
            await _db.SaveChangesAsync();

            return Ok(new { AccessToken = tokens.AccessToken, RefreshToken = tokens.RefreshToken });
        }

        private (string AccessToken, string RefreshToken) GenerateTokens(User user)
        {
            var userInDb = _db.User.SingleOrDefault(u => u.Email == user.Email);
            var existinState = _db.Roles.Find(userInDb.RoleId);

            var claims = new[]
            {
                 new Claim(ClaimTypes.Name, user.Email),
                 new Claim(ClaimTypes.Role, existinState.Name),
                 new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())
             };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSecretKeyWithAtLeast16Characters"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds);

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            return (new JwtSecurityTokenHandler().WriteToken(accessToken), refreshToken);
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            var userInDb = await _db.User.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > DateTime.UtcNow);
            if (userInDb == null)
            {
                return BadRequest("Invalid or expired refresh token.");
            }


            var newTokens = GenerateTokens(userInDb);
            userInDb.RefreshToken = newTokens.RefreshToken;
            await _db.SaveChangesAsync();

            return Ok(new { AccessToken = newTokens.AccessToken, RefreshToken = newTokens.RefreshToken });
        }

    }
}