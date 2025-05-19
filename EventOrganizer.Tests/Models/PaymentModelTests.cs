using Xunit;
using EventOrganizer.Models;
using System;

namespace EventOrganizer.Tests.Models
{
    public class PaymentModelTests
    {
        [Fact]
        public void Payment_CanBeInitialized_WithValidData()
        {
            var payment = new Payment
            {
                Id = 1,
                Name = "John",
                Surname = "Doe",
                Phone = "123456789",
                Date = new DateTime(2024, 1, 1),
                Amount = "100.00",
                ReservationId = 10
            };

            Assert.Equal(1, payment.Id);
            Assert.Equal("John", payment.Name);
            Assert.Equal("Doe", payment.Surname);
            Assert.Equal("123456789", payment.Phone);
            Assert.Equal(new DateTime(2024, 1, 1), payment.Date);
            Assert.Equal("100.00", payment.Amount);
            Assert.Equal(10, payment.ReservationId);
        }
    }
} 