using EventOrganizer.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;  // Add this import for EPPlus
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventOrganizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly IMongoCollection<Restaurants> _restaurants;
        private readonly IMongoCollection<RestaurantTypes> _restaurantTypes;

        public RestaurantsController(IMongoClient client)
        {
            var database = client.GetDatabase("OrganizingEvents");
            _restaurants = database.GetCollection<Restaurants>("Restaurants");
            _restaurantTypes = database.GetCollection<RestaurantTypes>("RestaurantTypes");
        }

        // Get All List
        [HttpGet("GetAllList")]
        public async Task<IActionResult> GetAllListAsync()
        {
            var restaurants = await _restaurants.Find(_ => true).ToListAsync();

            if (restaurants == null || restaurants.Count == 0)
            {
                return NotFound("No restaurants found.");
            }

            // Get all distinct RestaurantTypesIds from the restaurants
            var restaurantTypeIds = restaurants.Select(r => r.RestaurantTypesId).Distinct().ToList();
            var restaurantTypes = await _restaurantTypes.Find(rt => restaurantTypeIds.Contains(rt.Id)).ToListAsync();

            // Map restaurantType to each restaurant
            var result = restaurants.Select(r => new
            {
                r.Id,
                r.Name,
                r.Location,
                r.Image,
                r.Description,
                RestaurantType = restaurantTypes.FirstOrDefault(rt => rt.Id == r.RestaurantTypesId)
            });

            return Ok(result);
        }

        // Get By Id
        [HttpGet("GetRestaurantsById/{id:length(24)}", Name = "GetRestaurants")]
        public async Task<IActionResult> GetRestaurantsByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid ID format.");
            }

            var restaurant = await _restaurants.Find(r => r.Id == id).FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return NotFound($"Restaurant with ID '{id}' not found.");
            }

            var restaurantType = await _restaurantTypes.Find(rt => rt.Id == restaurant.RestaurantTypesId).FirstOrDefaultAsync();

            var result = new
            {
                restaurant.Id,
                restaurant.Name,
                restaurant.Location,
                restaurant.Image,
                restaurant.Description,
                RestaurantType = restaurantType
            };

            return Ok(result);
        }

        // Add
        [HttpPost("Add")]
        public async Task<IActionResult> CreateAsync(Restaurants restaurant)
        {
            // Check if RestaurantTypesId is valid
            if (!ObjectId.TryParse(restaurant.RestaurantTypesId.ToString(), out _))
            {
                return BadRequest($"RestaurantTypes Id '{restaurant.RestaurantTypesId}' is not in the correct format.");
            }

            var restaurantTypes = await _restaurantTypes.Find(rt => rt.Id == restaurant.RestaurantTypesId).FirstOrDefaultAsync();
            if (restaurantTypes == null)
            {
                return BadRequest($"The restaurantTypes with ID '{restaurant.RestaurantTypesId}' does not exist in the database.");
            }

            restaurant.Id = ObjectId.GenerateNewId().ToString(); // Generate a new ObjectId and convert it to string
            await _restaurants.InsertOneAsync(restaurant);
            return CreatedAtRoute("GetRestaurants", new { id = restaurant.Id }, restaurant);
        }

        // Update
        [HttpPut("Update/{id:length(24)}")]
        public async Task<IActionResult> UpdateAsync(string id, Restaurants updatedRestaurant)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid ID format.");
            }

            var filter = Builders<Restaurants>.Filter.Eq("_id", objectId);
            var existingRestaurant = await _restaurants.Find(filter).FirstOrDefaultAsync();

            if (existingRestaurant == null)
            {
                return NotFound($"Restaurant with ID '{id}' not found.");
            }

            // Update the existing restaurant with the data from updatedRestaurant
            existingRestaurant.Name = updatedRestaurant.Name;
            existingRestaurant.Location = updatedRestaurant.Location;
            existingRestaurant.Image = updatedRestaurant.Image;
            existingRestaurant.Description = updatedRestaurant.Description;
            existingRestaurant.RestaurantTypesId = updatedRestaurant.RestaurantTypesId;

            await _restaurants.ReplaceOneAsync(filter, existingRestaurant);
            return NoContent();
        }

        // Delete
        [HttpDelete("Delete/{id:length(24)}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid ID format.");
            }

            var filter = Builders<Restaurants>.Filter.Eq("_id", objectId);
            var existingRestaurant = await _restaurants.Find(filter).FirstOrDefaultAsync();

            if (existingRestaurant == null)
            {
                return NotFound($"Restaurant with ID '{id}' not found.");
            }

            await _restaurants.DeleteOneAsync(filter);
            return NoContent();
        }

        // Search Restaurants by Name
        [HttpGet("SearchRestaurant")]
        public async Task<IActionResult> SearchRestaurantAsync([FromQuery] string searchRestaurant)
        {
            if (string.IsNullOrEmpty(searchRestaurant))
            {
                return BadRequest("Search term cannot be empty.");
            }

            // Kërko restorantet ku emri përputhet me kërkimin
            var restaurants = await _restaurants
                .Find(r => r.Name.ToLower().Contains(searchRestaurant.ToLower()))
                .ToListAsync();

            if (restaurants == null || restaurants.Count == 0)
            {
                return NotFound("No matching restaurants found.");
            }

            return Ok(restaurants);
        }

        // Export Restaurants to Excel
        [HttpGet("ExportRestaurantsToExcel")]
        public async Task<IActionResult> ExportRestaurantsToExcel()
        {
            // Retrieve all restaurants
            var restaurants = await _restaurants.Find(_ => true).ToListAsync();

            // Get all distinct restaurant types to map them to restaurants
            var restaurantTypeIds = restaurants.Select(r => r.RestaurantTypesId).Distinct().ToList();
            var restaurantTypes = await _restaurantTypes.Find(rt => restaurantTypeIds.Contains(rt.Id)).ToListAsync();

            // Map restaurant types to each restaurant
            var restaurantsWithTypes = restaurants.Select(r => new
            {
                r.Id,
                r.Name,
                r.Location,
                r.Image,
                r.Description,
                RestaurantType = restaurantTypes.FirstOrDefault(rt => rt.Id == r.RestaurantTypesId)?.Name
            }).ToList();

            // Initialize an Excel package to generate the Excel file
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Restaurants");

                // Add headers to the Excel sheet
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "Location";
                worksheet.Cells[1, 4].Value = "Image";
                worksheet.Cells[1, 5].Value = "Description";
                worksheet.Cells[1, 6].Value = "Restaurant Type";

                // Add data to the Excel sheet
                for (int i = 0; i < restaurantsWithTypes.Count; i++)
                {
                    var restaurant = restaurantsWithTypes[i];
                    worksheet.Cells[i + 2, 1].Value = restaurant.Id;
                    worksheet.Cells[i + 2, 2].Value = restaurant.Name;
                    worksheet.Cells[i + 2, 3].Value = restaurant.Location;
                    worksheet.Cells[i + 2, 4].Value = restaurant.Image;
                    worksheet.Cells[i + 2, 5].Value = restaurant.Description;
                    worksheet.Cells[i + 2, 6].Value = restaurant.RestaurantType;
                }

                // Return the Excel file as a downloadable response
                var excelFileContent = package.GetAsByteArray();
                return File(excelFileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Restaurants.xlsx");
            }
        }
    }
}
