using EventOrganizer.Models;
using EventOrganizer.Contracts;
using EventOrganizer.Models;
using Stripe;

namespace EventOrganizer.Application
{
    public class StripeAppService : IStripeAppService
    {
        private readonly ChargeService _chargeService;
        private readonly CustomerService _customerService;
        private readonly TokenService _tokenService;

        public StripeAppService(
            ChargeService chargeService,
            CustomerService customerService,
            TokenService tokenService)
        {
            _chargeService = chargeService;
            _customerService = customerService;
            _tokenService = tokenService;
        }

        public async Task<StripePayment> AddStripePaymentAsync(AddStripePayment payment, CancellationToken ct)
        {
            try
            {
                // Create a token using TokenService
                var tokenOptions = new TokenCreateOptions
                {
                    Card = new TokenCardOptions
                    {
                        Number = payment.CardNumber,
                        ExpYear = payment.ExpirationYear,
                        ExpMonth = payment.ExpirationMonth,
                        Cvc = payment.Cvc
                    }
                };

                var tokenService = new TokenService();
                var token = await tokenService.CreateAsync(tokenOptions);

                // Log token creation
                Console.WriteLine($"Token created successfully: {token.Id}");

                // Set the options for the customer creation at Stripe
                var customerOptions = new CustomerCreateOptions
                {
                    Email = payment.Email,
                    Source = token.Id // Use the token ID as the source
                };

                // Create the customer
                var createdCustomer = await _customerService.CreateAsync(customerOptions, null, ct);

                // Log customer creation
                Console.WriteLine($"Customer created successfully: {createdCustomer.Id}");

                // Set the options for the payment we would like to create at Stripe
                ChargeCreateOptions paymentOptions = new ChargeCreateOptions
                {
                    Customer = createdCustomer.Id, // Use the customer ID for the payment
                    ReceiptEmail = payment.Email,
                    Description = payment.Description,
                    Currency = payment.Currency,
                    Amount = payment.Amount
                };

                // Create the payment
                var createdPayment = await _chargeService.CreateAsync(paymentOptions, null, ct);

                // Log payment creation
                Console.WriteLine($"Payment created successfully: {createdPayment.Id}");

                // Return the payment to the requesting method
                return new StripePayment(
                    payment.CardNumber,
                    payment.ExpirationYear,
                    payment.ExpirationMonth,
                    payment.Cvc,
                    payment.Email,
                    payment.Description,
                    payment.Currency,
                    payment.Amount,
                    createdPayment.Id);
            }
            catch (Exception ex)
            {
                // Log any exceptions
                Console.WriteLine($"Error: {ex.Message}");
                throw; // Re-throw the exception to handle it at a higher level
            }
        }
    }
}
