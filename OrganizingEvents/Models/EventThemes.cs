using System.ComponentModel.DataAnnotations;

namespace OrganizingEvents.Models
{
    public class EventThemes
    {
        [Key]
        public int Id { get; set; }
        public string ThemeName { get; set; }
        public string Description { get; set; }
    }
}
