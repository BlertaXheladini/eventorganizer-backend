using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using EventOrganizer.Database;
using EventOrganizer.Models;
using OfficeOpenXml; // Add this to work with EPPlus

namespace EventOrganizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public UsersController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find the user by email
            var user = await _db.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                // Do not    reveal user existence to prevent info leakage
                return Ok("Password reset email sent.");
            }

            // Generate a token - example using a GUID (you can use JWT or other token)
            var token = Guid.NewGuid().ToString();

            // Save the token to the user record, or to a separate password reset tokens table
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddHours(1); // token valid for 1 hour
            await _db.SaveChangesAsync();

            // Generate a callback URL including the userId and token
            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { userId = user.Id, token = token }, protocol: Request.Scheme);

            // Send email with the reset link
            await _emailSender.SendEmailAsync(
                email,
                "Reset Your Password",
                $@"Please reset your password by clicking here:
                <a href='{callbackUrl}'>Reset Password</a>");

            return Ok("Password reset email sent.");
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
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
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
            // Get all users with their roles
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

            // Create a new Excel package using EPPlus
            using (var package = new ExcelPackage())
            {
                // Add a worksheet to the package
                var worksheet = package.Workbook.Worksheets.Add("Users");

                // Add headers to the first row
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "First Name";
                worksheet.Cells[1, 3].Value = "Last Name";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Role";
                worksheet.Cells[1, 6].Value = "Refresh Token";
                worksheet.Cells[1, 7].Value = "Refresh Token Expiry Time";

                // Add data to the worksheet
                int row = 2; // Starting from the second row
                foreach (var user in users)
                {
                    worksheet.Cells[row, 1].Value = user.Id;
                    worksheet.Cells[row, 2].Value = user.FirstName;
                    worksheet.Cells[row, 3].Value = user.LastName;
                    worksheet.Cells[row, 4].Value = user.Email;
                    worksheet.Cells[row, 5].Value = user.RoleName;
                    worksheet.Cells[row, 6].Value = user.RefreshToken;
                    worksheet.Cells[row, 7].Value = user.RefreshTokenExpiryTime?.ToString("yyyy-MM-dd HH:mm:ss"); // Format the date if necessary
                    row++;
                }

                // Return the Excel file as a byte array
                var excelFileContent = package.GetAsByteArray();
                return File(excelFileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
            }
        }
    }
}
