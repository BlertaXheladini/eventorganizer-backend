using OrganizingEvents.Models;

namespace OrganizingEvents.Contracts
{
    public interface IStripeAppService
    {
        Task<StripePayment> AddStripePaymentAsync(AddStripePayment payment, CancellationToken ct);
    }
}
