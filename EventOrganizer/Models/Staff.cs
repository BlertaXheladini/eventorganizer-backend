using System.ComponentModel.DataAnnotations;

namespace EventOrganizer.Models  // Fixed the namespace here
{
    public class Staff
    {
        [Key]
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Position { get; set; }

        public string ContactNumber { get; set; }

        public string Image { get; set; }
    }
}
