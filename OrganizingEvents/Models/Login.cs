using System.ComponentModel.DataAnnotations;

namespace OrganizingEvents.Models
{
    public class Login
    {
        [EmailAddress]
        public string Email { get; set; }
        [StringLength(20,MinimumLength = 2)]
        public string Password { get; set; }
    }
}
