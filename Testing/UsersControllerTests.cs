using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizingEvents.Models;
using OrganizingEvents.Controllers;
using OrganizingEvents.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Testing
{
    public class UsersControllerTests
    {
        private ApplicationDbContext GetDbContextWithData()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "UsersTestDb")
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsUser()
        {
            // Arrange
            var context = GetDbContextWithData();
            var controller = new UsersController(context);
            var testUser = new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com" };
            context.User.Add(testUser);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetUserById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var user = Assert.IsType<User>(okResult.Value);
            Assert.Equal(testUser.Email, user.Email);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var context = GetDbContextWithData();
            var controller = new UsersController(context);

            // Act
            var result = await controller.GetUserById(2023202334);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Create_WithValidUser_ReturnsCreatedUser()
        {
            // Arrange
            var context = GetDbContextWithData();
            var controller = new UsersController(context);
            var newUser = new User { FirstName = "New", LastName = "User", Email = "new@test.com" };

            // Act
            var result = await controller.PostAsync(newUser);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            var user = Assert.IsType<User>(createdResult.Value);
            Assert.Equal(newUser.Email, user.Email);
        }
    }
} 