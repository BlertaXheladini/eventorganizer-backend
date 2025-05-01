namespace EventOrganizer.Models
{
    public record AddStripePayment(
   string CardNumber,
   string ExpirationYear,
   string ExpirationMonth,
   string Cvc,
   string Email,
   string Description,
   string Currency,
   long Amount
);
}