using System.ComponentModel.DataAnnotations;

namespace OrganizingEvents.Models
{
    public class Staff
    {
        [Key]
        public int Id { get; set; }

        public string FistName { get; set; }

        public string LastName { get; set; }

        public string Position { get; set; }

        public string ContactNumber { get; set; }

        public string Image { get; set; }

    }
}