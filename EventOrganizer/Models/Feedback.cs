using System.ComponentModel.DataAnnotations;

using EventOrganizer.Models;

namespace EventOrganizer.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Name { get; set; }

        public string Surname { get; set; }

        public string Comments { get; set; }

        [Required]
        public int EventsId { get; set; }

        public Events Events { get; set; }
    }
}