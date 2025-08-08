using Domain.Models;

namespace Domain.Interfaces
{
    public interface IExchangeProvider
    {
        string Name { get; }
        Task<OfferResult?> GetOfferAsync(ExchangeRequest request, CancellationToken ct);
    }
}
