using System.ComponentModel.DataAnnotations;

namespace EventOrganizer.Models
{
    public class EventThemes
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ThemeName { get; set; }

        public string? Description { get; set; }
    }
}
