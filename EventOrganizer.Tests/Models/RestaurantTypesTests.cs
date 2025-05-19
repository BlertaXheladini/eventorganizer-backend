using Xunit;
using EventOrganizer.Models;

namespace EventOrganizer.Tests.Models
{
    public class RestaurantTypesTests
    {
        [Fact]
        public void RestaurantTypes_CanBeInitialized_WithValidData()
        {
            var restaurantType = new RestaurantTypes
            {
                Id = "123456789012345678901234",
                Name = "Italian",
                Description = "Italian cuisine"
            };

            Assert.Equal("123456789012345678901234", restaurantType.Id);
            Assert.Equal("Italian", restaurantType.Name);
            Assert.Equal("Italian cuisine", restaurantType.Description);
        }
    }
} 