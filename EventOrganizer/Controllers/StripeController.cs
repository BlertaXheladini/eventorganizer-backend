using Microsoft.AspNetCore.Mvc;
using Stripe;
using Microsoft.Extensions.Configuration;

namespace EventOrganizer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public StripeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("CreatePaymentIntent")]
        public ActionResult CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            // Vendosni çelësin sekret të Stripe
            StripeConfiguration.ApiKey = "sk_test_51Q0jhrP7aD7Wb7DXHbRmFIMzZ59pOBRBUnpGqv1hjdycWZfCkT9x3NidYe02lv0Iwv6i0R8r1u3QL91MFCiPHcGR00wKWe7wY8";


            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100), // Konvertoni në cent
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
                Description = request.Description,
            };

            var service = new PaymentIntentService();
            var paymentIntent = service.Create(options);

            return Ok(new { clientSecret = paymentIntent.ClientSecret });
        }
    }

    // Krijoni klasën për kërkesën
    public class CreatePaymentIntentRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}