using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizingEvents.Models;
using OrganizingEvents.Controllers;
using OrganizingEvents.Data;

namespace Testing
{
    public class UnitTest1
    {

        private ApplicationDbContext GetDbContextWithData()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "OrganizingEvents")
                .Options;

            var context = new ApplicationDbContext(options);

            // Seed some users
            context.User.AddRange(
                new User { Id = 1, FirstName = "Blerta" },
                new User { Id = 2, FirstName = "Vjollca" }
            );
            context.SaveChanges();

            return context;
        }
        [Fact]
        public async Task GetAsync_ReturnsAllUsers()
        {
            // Arrange
            var context = GetDbContextWithData();
            var controller = new UsersController(context);

            // Act
            var result = await controller.GetAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var users = Assert.IsAssignableFrom<List<User>>(okResult.Value);
            Assert.Equal(2, users.Count);
            Assert.Contains(users, u => u.FirstName == "Blerta");
            Assert.Contains(users, u => u.FirstName == "Vjollca");
        }

    }

}
