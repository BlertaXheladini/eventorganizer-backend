using System.ComponentModel.DataAnnotations;

namespace OrganizingEvents.Models
{
    public class EventCategories
    {
        [Key]
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }
}
