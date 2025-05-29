using OrganizingEvents.Data;
using OrganizingEvents.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Azure.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
namespace OrganizingEvents.Controllers
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

        private static Dictionary<string, (int Attempts, DateTime? LockoutEnd)> _loginAttempts = new();

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(Login user)
        {
            string key = user.Email.ToLower();
            if (_loginAttempts.TryGetValue(key, out var attempt))
            {
                if (attempt.LockoutEnd.HasValue && attempt.LockoutEnd.Value > DateTime.UtcNow)
                {
                    return BadRequest("Llogaria është bllokuar përkohësisht. Provoni më vonë.");
                }
            }

            var userInDb = await _db.User.SingleOrDefaultAsync(u => u.Email == user.Email);
            if (userInDb == null || !BCrypt.Net.BCrypt.Verify(user.Password, userInDb.Password))
            {
                if (!_loginAttempts.ContainsKey(key))
                    _loginAttempts[key] = (1, null);
                else
                {
                    var newAttempts = _loginAttempts[key].Attempts + 1;
                    var lockout = newAttempts >= 5 ? DateTime.UtcNow.AddMinutes(15) : (DateTime?)null;
                    _loginAttempts[key] = (newAttempts, lockout);
                }

                return BadRequest("Emaili ose Fjalëkalimi gabim.");
            }

            // Reset attempts on successful login
            _loginAttempts.Remove(key);

            var tokens = GenerateTokens(userInDb);
            userInDb.RefreshToken = tokens.RefreshToken;
            userInDb.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                RoleId = userInDb.RoleId,
                UserId = userInDb.Id,
                FirstName = userInDb.FirstName,
                LastName = userInDb.LastName
            });
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

        [HttpGet]
        [Route("ExportUsersToExcel")]
        public async Task<IActionResult> ExportUsersToExcel()
        {
            // Merr të gjithë përdoruesit bashkë me rolet e tyre
            var users = await _db.User
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Password,
                    RoleName = u.Role.Name,
                    u.RefreshToken,
                    u.RefreshTokenExpiryTime
                })
                .ToListAsync();

            var excelFileContent = ExcelExporter.ExportToExcel(users);

            return File(excelFileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
        }


        [HttpPost]
        [Route("request-reset-code")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
        {
            var user = await _db.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found");

            string token = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(15);
            await _db.SaveChangesAsync();

            var emailService = new EmailService();
            emailService.SendResetCode(email, token);

            return Ok("Password reset code sent to email.");
        }


        [HttpPost]
        [Route("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode([FromBody] ResetCodeDto dto)
        {
            var user = await _db.User.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || user.PasswordResetToken != dto.Code || user.PasswordResetTokenExpiryTime < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired code");
            }

            return Ok("Code is valid");
        }

        public class ResetCodeDto
        {
            public string Email { get; set; }
            public string Code { get; set; }
        }

        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _db.User.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || user.PasswordResetToken != dto.Code || user.PasswordResetTokenExpiryTime < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired code");
            }

            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, salt);

            user.Password = hashedPassword;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiryTime = null;

            await _db.SaveChangesAsync();

            return Ok("Password has been reset.");
        }

        public class ResetPasswordDto
        {
            public string Email { get; set; }
            public string Code { get; set; }
            public string NewPassword { get; set; }
        }

    }
}
