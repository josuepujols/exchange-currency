

namespace Domain.Models
{
    public class OfferResult
    {
        public string? Provider { get; set; }
        public decimal Rate { get; set; }
        public long LatencyMs { get; set; }
    }
}
