using EventOrganizer.Models;
using System.ComponentModel.DataAnnotations;

namespace OrganizingEvents.Models
{
    public class Events
    {
        [Key]
        public int Id { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }

        public DateTime Date { get; set; }

        public string Location { get; set; }

        public int Capacity { get; set; }
        public string Image { get; set; }

        public string Price { get; set; }

        public int CategoryId { get; set; }
        public int ThemeId { get; set; }
        public EventCategories EventCategories { get; set; }

        public EventThemes EventThemes { get; set; }

    }
}