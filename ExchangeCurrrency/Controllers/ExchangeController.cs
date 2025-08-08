using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeCurrrency.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeController : Controller
    {
        private readonly ICompareRatesService _compareRatesService;
        public ExchangeController(ICompareRatesService compareRatesService)
            => _compareRatesService = compareRatesService;


        /// <summary>
        /// Compares exchange rate offers and returns the best deal.
        /// </summary>
        /// <param name="request">Exchange request parameters</param>
        /// <returns>Best offer found</returns>
        /// <response code="200">Returns the best offer</response>
        /// <response code="502">If there is a problem with the providers</response>
        [HttpPost]
        public async Task<IActionResult> BestBid([FromBody] ExchangeRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var best = await _compareRatesService.GetBestOfferAsync(request, cancellationToken);
                return Ok(best);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status502BadGateway);
            }
        }
    }
}
