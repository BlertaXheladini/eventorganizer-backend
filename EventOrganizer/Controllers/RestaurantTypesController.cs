using EventOrganizer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using EventOrganizer.Models;

namespace EventOrganizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantTypesController : ControllerBase
    {
        private readonly IMongoCollection<RestaurantTypes> _restaurantTypes;

        public RestaurantTypesController(IMongoClient client)
        {
            var database = client.GetDatabase("OrganizingEvents");
            _restaurantTypes = database.GetCollection<RestaurantTypes>("RestaurantTypes");
        }

        //Get
        [HttpGet("GetAllList")]
        public ActionResult<List<RestaurantTypes>> Get() =>
           _restaurantTypes.Find(restaurants => true).ToList();


        [HttpGet("GetRestaurantTypesById/{id:length(24)}", Name = "GetRestaurantTypes")]
        public ActionResult<RestaurantTypes> Get(string id)
        {
            var restaurantTypes = _restaurantTypes.Find<RestaurantTypes>(o => o.Id == id).FirstOrDefault();

            if (restaurantTypes == null)
            {
                return NotFound();
            }

            return restaurantTypes;
        }

        //Post
        [HttpPost("Add")]
        public ActionResult<RestaurantTypes> Create(RestaurantTypes restaurant)
        {
            try
            {
                // Generate a unique ID
                restaurant.Id = ObjectId.GenerateNewId().ToString(); // Generate a unique ObjectId and convert it to string
                _restaurantTypes.InsertOne(restaurant);
                return CreatedAtRoute("GetRestaurantTypes", new { id = restaurant.Id }, restaurant);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //Put
        [HttpPut("Update/{id:length(24)}")]
        public IActionResult Update(string id, RestaurantTypes updatedRestaurantTypes)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    return BadRequest("ID nuk është në formatin e duhur.");
                }

                var filter = Builders<RestaurantTypes>.Filter.Eq("_id", objectId);
                var restaurant = _restaurantTypes.Find(filter).FirstOrDefault();

                if (restaurant == null)
                {
                    return NotFound();
                }

                _restaurantTypes.ReplaceOne(filter, updatedRestaurantTypes);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //Delete
        [HttpDelete("Delete/{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    return BadRequest("ID nuk është në formatin e duhur.");
                }

                var filter = Builders<RestaurantTypes>.Filter.Eq("_id", objectId);
                var restaurant = _restaurantTypes.Find(filter).FirstOrDefault();

                if (restaurant == null)
                {
                    return NotFound();
                }

                _restaurantTypes.DeleteOne(filter);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}