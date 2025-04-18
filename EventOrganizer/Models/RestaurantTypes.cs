﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EventOrganizer.Models
{
    public class RestaurantTypes
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
