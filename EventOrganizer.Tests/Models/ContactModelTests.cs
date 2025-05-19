using Xunit;
using EventOrganizer.Models;

namespace EventOrganizer.Tests.Models
{
    public class ContactModelTests
    {
        [Fact]
        public void Contact_CanBeInitialized_WithValidData()
        {
            var contact = new Contact
            {
                Id = 1,
                Name = "Jane Doe",
                Email = "jane@example.com",
                Message = "Hello, this is a test message."
            };

            Assert.Equal(1, contact.Id);
            Assert.Equal("Jane Doe", contact.Name);
            Assert.Equal("jane@example.com", contact.Email);
            Assert.Equal("Hello, this is a test message.", contact.Message);
        }
    }
} 