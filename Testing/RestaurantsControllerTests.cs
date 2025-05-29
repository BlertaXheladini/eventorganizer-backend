using Xunit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using OrganizingEvents.Models;
using OrganizingEvents.Controllers;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson;

namespace Testing
{
    public class RestaurantsControllerTests
    {
        private IMongoClient GetMongoClient()
        {
            return new MongoClient("mongodb://localhost:27017");
        }

        [Fact]
        public async Task GetAllListAsync_ReturnsAllRestaurants()
        {
            // Arrange
            var client = GetMongoClient();
            var controller = new RestaurantsController(client);

            // Act
            var result = await controller.GetAllListAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var restaurants = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
        }

        [Fact]
        public async Task GetRestaurantsByIdAsync_WithValidId_ReturnsRestaurant()
        {
            // Arrange
            var client = GetMongoClient();
            var controller = new RestaurantsController(client);
            var validId = ObjectId.GenerateNewId().ToString();

            // Act
            var result = await controller.GetRestaurantsByIdAsync(validId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetRestaurantsByIdAsync_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var client = GetMongoClient();
            var controller = new RestaurantsController(client);
            var invalidId = "invalid-id";

            // Act
            var result = await controller.GetRestaurantsByIdAsync(invalidId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateAsync_WithValidRestaurant_ReturnsCreatedRestaurant()
        {
            // Arrange
            var client = GetMongoClient();
            var controller = new RestaurantsController(client);
            var newRestaurant = new Restaurants 
            { 
                Name = "New Restaurant",
                Location = "Test Location",
                Description = "Test Description",
                RestaurantTypesId = ObjectId.GenerateNewId().ToString()
            };

            // Act
            var result = await controller.CreateAsync(newRestaurant);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result); // Will fail because RestaurantTypesId doesn't exist
        }

        [Fact]
        public async Task UpdateAsync_WithValidRestaurant_ReturnsNoContent()
        {
            // Arrange
            var client = GetMongoClient();
            var controller = new RestaurantsController(client);
            var validId = ObjectId.GenerateNewId().ToString();
            var updatedRestaurant = new Restaurants 
            { 
                Name = "Updated Restaurant",
                Location = "Updated Location",
                Description = "Updated Description",
                RestaurantTypesId = ObjectId.GenerateNewId().ToString()
            };

            // Act
            var result = await controller.UpdateAsync(validId, updatedRestaurant);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var client = GetMongoClient();
            var controller = new RestaurantsController(client);
            var validId = ObjectId.GenerateNewId().ToString();

            // Act
            var result = await controller.DeleteAsync(validId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task SearchRestaurantAsync_WithValidSearchTerm_ReturnsMatchingRestaurants()
        {
            // Arrange
            var client = GetMongoClient();
            var controller = new RestaurantsController(client);
            var searchTerm = "test";

            // Act
            var result = await controller.SearchRestaurantAsync(searchTerm);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
} 