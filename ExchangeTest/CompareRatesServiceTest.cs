using Application.Services;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExchangeTest
{
    [TestClass]
    public class CompareRatesServiceTest
    {
        [TestMethod]
        public async Task GetBestOfferAsync_ReturnsBestOffer_WhenProvidersReturnResults()
        {
            // Arrange
            var cts = new CancellationTokenSource();

            var provider1Mock = new Mock<IExchangeProvider>();
            provider1Mock.Setup(p => p.Name).Returns("Provider1");
            provider1Mock.Setup(p => p.GetOfferAsync(It.IsAny<ExchangeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OfferResult { Provider = "Provider1", Rate = 110, LatencyMs = 1234 });

            var provider2Mock = new Mock<IExchangeProvider>();
            provider2Mock.Setup(p => p.Name).Returns("Provider2");
            provider2Mock.Setup(p => p.GetOfferAsync(It.IsAny<ExchangeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OfferResult { Provider = "Provider2", Rate = 120, LatencyMs = 12346 });

            var provider3Mock = new Mock<IExchangeProvider>();
            provider2Mock.Setup(p => p.Name).Returns("Provider2");
            provider2Mock.Setup(p => p.GetOfferAsync(It.IsAny<ExchangeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OfferResult { Provider = "Provider3", Rate = 150, LatencyMs = 123467 });

            var providers = new List<IExchangeProvider> { provider1Mock.Object, provider2Mock.Object, provider3Mock.Object };

            var loggerMock = new Mock<ILogger<CompareRatesService>>();

            var service = new CompareRatesService(providers, loggerMock.Object, TimeSpan.FromSeconds(5));

            var request = new ExchangeRequest
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 50
            };

            // Act
            var bestOffer = await service.GetBestOfferAsync(request, cts.Token);

            // Assert
            Assert.IsNotNull(bestOffer);
            Assert.AreEqual("Provider3", bestOffer.Provider);
            Assert.AreEqual(150, bestOffer.Rate);

            provider1Mock.Verify(p => p.GetOfferAsync(It.IsAny<ExchangeRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            provider2Mock.Verify(p => p.GetOfferAsync(It.IsAny<ExchangeRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            provider3Mock.Verify(p => p.GetOfferAsync(It.IsAny<ExchangeRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetBestOfferAsync_Returns_0_AsBestOffer_WhenProviderReturns_0_AsResult()
        {
            // Arrange
            var cts = new CancellationTokenSource();

            var provider1Mock = new Mock<IExchangeProvider>();
            provider1Mock.Setup(p => p.Name).Returns("Provider1");
            provider1Mock.Setup(p => p.GetOfferAsync(It.IsAny<ExchangeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OfferResult { Provider = "Provider1", Rate = 0, LatencyMs = 1234 });

            var providers = new List<IExchangeProvider> { provider1Mock.Object };

            var loggerMock = new Mock<ILogger<CompareRatesService>>();

            var service = new CompareRatesService(providers, loggerMock.Object, TimeSpan.FromSeconds(5));

            var request = new ExchangeRequest
            {
                FromCurrency = "JPY",
                ToCurrency = "BGN",
                Amount = 50
            };

            // Act
            var bestOffer = await service.GetBestOfferAsync(request, cts.Token);

            // Assert
            Assert.IsNotNull(bestOffer);
            Assert.AreEqual("Provider1", bestOffer.Provider);
            Assert.AreEqual(0, bestOffer.Rate);

            provider1Mock.Verify(p => p.GetOfferAsync(It.IsAny<ExchangeRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetBestOfferAsync_Throws_WhenNoProvidersReturnValidOffer()
        {
            // Arrange
            var cts = new CancellationTokenSource();

            var providerMock = new Mock<IExchangeProvider>();
            providerMock.Setup(p => p.Name).Returns("BadProvider");
            providerMock.Setup(p => p.GetOfferAsync(It.IsAny<ExchangeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((OfferResult?)null);

            var providers = new List<IExchangeProvider> { providerMock.Object };

            var loggerMock = new Mock<ILogger<CompareRatesService>>();

            var service = new CompareRatesService(providers, loggerMock.Object, TimeSpan.FromSeconds(5));

            var request = new ExchangeRequest
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100
            };

            await service.GetBestOfferAsync(request, cts.Token);
        }
    }
}
