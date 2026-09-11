namespace Reinsurance.Services.Exposure.Models
{
    public sealed class ExposureRecordModel
    {
        public int Id { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string PerilCode { get; set; }
        public string PerilName { get; set; }
        public decimal TotalInsuredValue { get; set; }
        public int RiskCount { get; set; }
        public decimal AverageDeductiblePct { get; set; }
    }
}
