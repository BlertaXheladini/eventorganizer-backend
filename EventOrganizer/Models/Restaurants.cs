using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EventOrganizer.Models
{
    public class Restaurants
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public string Image { get; set; }

        public string Description { get; set; }

        public string RestaurantTypesId { get; set; }
    }
}
