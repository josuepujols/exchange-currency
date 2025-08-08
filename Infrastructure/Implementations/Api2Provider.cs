using Domain.Interfaces;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Infrastructure.Implementations
{
    public class Api2Provider : IExchangeProvider
    {
        private readonly HttpClient _client;
        public string Name => "Api2";

        public Api2Provider(HttpClient client)
            => _client = client;

        public async Task<OfferResult?> GetOfferAsync(ExchangeRequest request, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var url = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
                using var resp = await _client.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode) return null;

                var xmlString = await resp.Content.ReadAsStringAsync(ct);
                var doc = XDocument.Parse(xmlString);

                XNamespace gesmes = "http://www.gesmes.org/xml/2002-08-01";
                XNamespace ns = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";

                var cubeRoot = doc.Root
                    ?.Element(ns + "Cube")
                    ?.Element(ns + "Cube");

                if (cubeRoot == null) return null;

                //Buscamos los rates validos para evitar errores
                var ratesDict = cubeRoot.Elements(ns + "Cube")
                    .Where(x => x.Attribute("currency") != null && x.Attribute("rate") != null)
                    .ToDictionary(
                        x => x.Attribute("currency")!.Value,
                        x => decimal.Parse(x.Attribute("rate")!.Value, CultureInfo.InvariantCulture)
                    );

                // Le asignamos un valo por defecto al rate porque hay que hacer la conversion siempre ne base al EUR debido al API
                ratesDict["EUR"] = 1m;

                var fromCurrency = request.FromCurrency.ToUpperInvariant();
                var toCurrency = request.ToCurrency.ToUpperInvariant();

                if (!ratesDict.ContainsKey(fromCurrency) || !ratesDict.ContainsKey(toCurrency))
                    return null;

                var rateFrom = ratesDict[fromCurrency];
                var rateTo = ratesDict[toCurrency];

                var conversionRate = rateTo / rateFrom;
                var total = request.Amount * conversionRate;

                return new OfferResult
                {
                    Provider = Name,
                    Rate = total,
                    LatencyMs = sw.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
