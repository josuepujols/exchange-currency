using Domain.Interfaces;
using Domain.Models;
using System.Diagnostics;
using System.Net.Http.Json;

namespace Infrastructure.Implementations
{
    public class Api1Provider : IExchangeProvider
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;
        public string Name => "Api1";

        public Api1Provider(HttpClient client, string ApiKey)
        {
            _client = client;
            _apiKey = ApiKey;
        }

        public async Task<OfferResult?> GetOfferAsync(ExchangeRequest request, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var url = $"/fetch-one?from={request.FromCurrency}&to={request.ToCurrency}&api_key={_apiKey}";

                using var resp = await _client.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode) return null;

                var body = await resp.Content.ReadFromJsonAsync<FastForexResponse>(cancellationToken: ct);
                if (body == null || !body.Result.ContainsKey(request.ToCurrency)) return null;

                // Buscamos el rate de la moneda a convertir
                var rate = body.Result[request.ToCurrency];
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
                throw;
            }
        }

        private class FastForexResponse
        {
            public Dictionary<string, decimal> Result { get; set; } = new Dictionary<string, decimal>();
        }
    }
}

