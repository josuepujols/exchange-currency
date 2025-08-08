using Domain.Interfaces;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Implementations
{
    public class Api3Provider : IExchangeProvider
    {
        private readonly HttpClient _client;
        public string Name => "Api3";

        public Api3Provider(HttpClient client)
            => _client = client;

        public async Task<OfferResult?> GetOfferAsync(ExchangeRequest request, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var url = $"latest/{request.FromCurrency}";

                var response = await _client.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                    return null;

                var data = await response.Content.ReadFromJsonAsync<ExchangeRateApiResponse>(cancellationToken: ct);
                if (data == null || data.result != "success" || data.conversion_rates == null)
                    return null;

                var toCurrency = request.ToCurrency.ToUpperInvariant();

                if (!data.conversion_rates.TryGetValue(toCurrency, out var rate))
                    return null;

                var total = request.Amount * rate;

                return new OfferResult
                {
                    Provider = Name,
                    Rate = total,
                    LatencyMs = sw.ElapsedMilliseconds
                };
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                return null;
            }
        }
        private class ExchangeRateApiResponse
        {
            public string result { get; set; } = "";
            public string documentation { get; set; } = "";
            public string terms_of_use { get; set; } = "";
            public long time_last_update_unix { get; set; }
            public string time_last_update_utc { get; set; } = "";
            public long time_next_update_unix { get; set; }
            public string time_next_update_utc { get; set; } = "";
            public string base_code { get; set; } = "";
            public Dictionary<string, decimal>? conversion_rates { get; set; }
        }
    }
}
