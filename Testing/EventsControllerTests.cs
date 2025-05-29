using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizingEvents.Models;
using OrganizingEvents.Controllers;
using OrganizingEvents.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Testing
{
    public class EventsControllerTests
    {
        private ApplicationDbContext GetDbContextWithData(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        [Fact]
        public async Task GetAsync_ReturnsAllEvents()
        {
            // Arrange
            var context = GetDbContextWithData("GetAllEventsTest");
            var controller = new EventsController(context);

            // Add test events
            var theme = new EventThemes { Id = 1, ThemeName = "Test Theme" };
            var category = new EventCategories { Id = 1, CategoryName = "Test Category" };
            context.EventThemes.Add(theme);
            context.EventCategories.Add(category);
            context.Events.AddRange(
                new Events { Id = 1, EventName = "Test Event 1", Description = "Test Description 1", ThemeId = 1, CategoryId = 1, Price = "100" },
                new Events { Id = 2, EventName = "Test Event 2", Description = "Test Description 2", ThemeId = 1, CategoryId = 1, Price = "200" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<List<Events>>(okResult.Value);
            Assert.Equal(2, events.Count);
        }

        [Fact]
        public async Task GetEventsByIdAsync_WithValidId_ReturnsEvent()
        {
            // Arrange
            var context = GetDbContextWithData("GetEventByIdTest");
            var controller = new EventsController(context);
            var theme = new EventThemes { Id = 1, ThemeName = "Test Theme" };
            var category = new EventCategories { Id = 1, CategoryName = "Test Category" };
            context.EventThemes.Add(theme);
            context.EventCategories.Add(category);
            var testEvent = new Events { Id = 1, EventName = "Test Event", Description = "Test Description", ThemeId = 1, CategoryId = 1, Price = "100" };
            context.Events.Add(testEvent);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetEventsByIdAsync(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var eventObj = Assert.IsType<Events>(okResult.Value);
            Assert.Equal(testEvent.EventName, eventObj.EventName);
        }

        [Fact]
        public async Task GetEventsByIdAsync_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var context = GetDbContextWithData("GetEventByIdInvalidTest");
            var controller = new EventsController(context);

            // Act
            var result = await controller.GetEventsByIdAsync(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PostAsync_WithValidEvent_ReturnsCreatedEvent()
        {
            // Arrange
            var context = GetDbContextWithData("PostEventTest");
            var controller = new EventsController(context);
            var theme = new EventThemes { Id = 1, ThemeName = "Test Theme" };
            var category = new EventCategories { Id = 1, CategoryName = "Test Category" };
            context.EventThemes.Add(theme);
            context.EventCategories.Add(category);
            await context.SaveChangesAsync();

            var newEvent = new Events 
            { 
                EventName = "New Event",
                Description = "Test Description",
                ThemeId = 1,
                CategoryId = 1,
                Price = "100"
            };

            // Act
            var result = await controller.PostAsync(newEvent);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            var eventObj = Assert.IsType<Events>(createdResult.Value);
            Assert.Equal(newEvent.EventName, eventObj.EventName);
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var context = GetDbContextWithData("DeleteEventTest");
            var controller = new EventsController(context);
            var theme = new EventThemes { Id = 1, ThemeName = "Test Theme" };
            var category = new EventCategories { Id = 1, CategoryName = "Test Category" };
            context.EventThemes.Add(theme);
            context.EventCategories.Add(category);
            var testEvent = new Events { Id = 1, EventName = "Test Event", Description = "Test Description", ThemeId = 1, CategoryId = 1, Price = "100" };
            context.Events.Add(testEvent);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteAsync(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
} 