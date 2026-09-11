namespace Reinsurance.Services.Exposure.Models
{
    public sealed class ExposureBreakdown
    {
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string PerilCode { get; set; }
        public string PerilName { get; set; }
        public decimal Tiv { get; set; }
        public decimal SharePct { get; set; }
    }
}
