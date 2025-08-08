using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CompareRatesService : ICompareRatesService
    {
        private readonly IEnumerable<IExchangeProvider> _providers;
        private readonly ILogger<CompareRatesService> _logger;
        private readonly TimeSpan _globalTimeout;

        public CompareRatesService(IEnumerable<IExchangeProvider> providers, ILogger<CompareRatesService> logger, TimeSpan? globalTimeout = null)
        {
            _providers = providers;
            _logger = logger;
            _globalTimeout = globalTimeout ?? TimeSpan.FromSeconds(15);
        }

        public async Task<OfferResult> GetBestOfferAsync(ExchangeRequest request, CancellationToken ct)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_globalTimeout);
            var token = cts.Token;

            var tasks = _providers.Select(async p =>
            {
                try
                {
                    return await p.GetOfferAsync(request, token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Provider {Provider} canceled or timed out", p.Name);
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Provider {Provider} failed", p.Name);
                    return null;
                }
            }).ToList();

            var results = await Task.WhenAll(tasks);

            var valid = results.Where(r => r != null).Cast<OfferResult>().ToList();
            if (!valid.Any())
                throw new InvalidOperationException("No provider returned a valid offer within timeout.");

            // Ordernar por rate mas alto, tambien se podria ordenar por el tiempo que duro el request luego de ordena por rate mas alto
            var best = valid
                .OrderByDescending(r => r.Rate)
                .First();

            _logger.LogInformation("Best provider: {Provider} rate={Rate}", best.Provider, best.Rate);
            return best;
        }
    }
}
