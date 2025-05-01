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
using Microsoft.AspNetCore.Authorization;


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
        public async Task<IActionResult> PutAsync(User updatedUser)
        {
            var existingUser = await _db.User.FindAsync(updatedUser.Id);

            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.FirstName = updatedUser.FirstName;
            existingUser.LastName = updatedUser.LastName;
            existingUser.Email = updatedUser.Email;
            existingUser.RoleId = updatedUser.RoleId;

            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                // Hash the new password
                string salt = BCrypt.Net.BCrypt.GenerateSalt();
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password, salt);
                existingUser.Password = hashedPassword;
            }

            _db.User.Update(existingUser);
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

        [HttpGet]
        [Route("GetAvailableRoles")]
        public async Task<IActionResult> GetAvailableRoles()
        {
            var roles = await _db.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(User objUser)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(objUser.Email))
                {
                    return BadRequest("Email is required");
                }
                
                if (string.IsNullOrEmpty(objUser.Password))
                {
                    return BadRequest("Password is required");
                }
                
                if (string.IsNullOrEmpty(objUser.FirstName))
                {
                    return BadRequest("First name is required");
                }
                
                if (string.IsNullOrEmpty(objUser.LastName))
                {
                    return BadRequest("Last name is required");
                }

                // Check if email already exists
                var dbuser = await _db.User.FirstOrDefaultAsync(u => u.Email == objUser.Email);
                if (dbuser != null)
                {
                    return BadRequest("Email already exists");
                }

                // Check if role exists
                var existingRole = await _db.Roles.FindAsync(objUser.RoleId);
                if (existingRole == null)
                {
                    // Get available roles to help the user
                    var availableRoles = await _db.Roles.ToListAsync();
                    var roleIds = string.Join(", ", availableRoles.Select(r => r.Id));
                    
                    return BadRequest($"Role with ID {objUser.RoleId} does not exist. Available role IDs are: {roleIds}");
                }

                // Set role
                objUser.Role = existingRole;
                objUser.RefreshTokenExpiryTime = DateTime.UtcNow;

                // Hash password
                string salt = BCrypt.Net.BCrypt.GenerateSalt();
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(objUser.Password, salt);
                objUser.Password = hashedPassword;

                // Add user to database
                _db.User.Add(objUser);
                await _db.SaveChangesAsync();
                
                return Ok("Registration successful");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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

            return Ok(new { AccessToken = tokens.AccessToken, RefreshToken = tokens.RefreshToken, RoleId = userInDb.RoleId });
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