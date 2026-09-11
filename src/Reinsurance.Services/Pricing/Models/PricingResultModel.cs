namespace Reinsurance.Services.Pricing.Models
{
    public sealed class PricingResultModel
    {
        public int TreatyLayerId { get; set; }
        public decimal TechnicalPremium { get; set; }
        public decimal ExpectedLoss { get; set; }
        public decimal ExpenseLoad { get; set; }
        public decimal RiskLoad { get; set; }
        public decimal RateOnLine { get; set; }
        public decimal LossCostPct { get; set; }
        public decimal ProfitMarginPct { get; set; }
        public bool ReferralRequired { get; set; }
        public string ReferralReasons { get; set; }
        public decimal OurShareLine { get; set; }
    }
}
