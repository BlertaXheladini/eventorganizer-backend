namespace EventOrganizer.Models
{
    public record StripePayment(
      string CardNumber,
      string ExpirationYear,
      string ExpirationMonth,
      string Cvc,
      string Email,
      string Description,
      string Currency,
      long Amount,
      string PaymentId);
}