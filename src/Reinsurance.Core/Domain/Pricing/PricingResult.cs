using System;
using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Pricing
{
    [DataContract]
    public partial class PricingResult : BaseEntity, IAuditable
    {
        [DataMember]
        public int TreatyLayerId { get; set; }

        [DataMember]
        public decimal TechnicalPremium { get; set; }

        [DataMember]
        public decimal ExpectedLoss { get; set; }

        [DataMember]
        public decimal ExpenseLoad { get; set; }

        [DataMember]
        public decimal RiskLoad { get; set; }

        [DataMember]
        public decimal RateOnLine { get; set; }

        [DataMember]
        public decimal LossCostPct { get; set; }

        [DataMember]
        public decimal ProfitMarginPct { get; set; }

        [DataMember]
        public bool ReferralRequired { get; set; }

        [DataMember]
        public string ReferralReasons { get; set; }

        [DataMember]
        public DateTime CalculatedOn { get; set; }

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOnUtc { get; set; } = DateTime.UtcNow;
        public virtual Treaties.TreatyLayer TreatyLayer { get; set; }
    }
}
