using System;
using System.Collections.Generic;

namespace Reinsurance.Core.Domain
{
    public enum SubmissionStatus { Received, InReview, Quoted, Bound, Declined, Withdrawn }
    public enum TreatyType { PropertyCatXoL, PerRiskXoL, QuotaShare, AggregateXoL }
    public enum TreatyStatus { Draft, Quoted, Bound, Expired }
    public enum ReferralRuleKind { MaxLayerLimit, MinRateOnLine, MaxRateOnLine, CedentRatingBelow, UnderwriterAuthority, PortfolioLimitBreach }

    public class Cedent : BaseEntity, IAuditable
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Country { get; set; }
        public string Rating { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }

    public class Broker : BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }

    public class Underwriter : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal AuthorityLimit { get; set; }
        public bool Active { get; set; }
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }

    public class Peril : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public virtual ICollection<ExposureRecord> ExposureRecords { get; set; } = new List<ExposureRecord>();
    }

    public class Region : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public virtual ICollection<ExposureRecord> ExposureRecords { get; set; } = new List<ExposureRecord>();
    }

    public class Submission : BaseEntity, IAuditable, ISoftDeletable
    {
        public string Reference { get; set; }
        public int CedentId { get; set; }
        public int? BrokerId { get; set; }
        public int? UnderwriterId { get; set; }
        public DateTime ReceivedOn { get; set; }
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public SubmissionStatus Status { get; set; }
        public string Notes { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public virtual Cedent Cedent { get; set; }
        public virtual Broker Broker { get; set; }
        public virtual Underwriter Underwriter { get; set; }
        public virtual ICollection<Treaty> Treaties { get; set; } = new List<Treaty>();
        public virtual ICollection<ExposureRecord> ExposureRecords { get; set; } = new List<ExposureRecord>();
        public virtual ICollection<CatModelResult> CatModelResults { get; set; } = new List<CatModelResult>();
    }

    public class Treaty : BaseEntity, IAuditable
    {
        public int SubmissionId { get; set; }
        public string Name { get; set; }
        public TreatyType Type { get; set; }
        public string Currency { get; set; }
        public TreatyStatus Status { get; set; }
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public virtual Submission Submission { get; set; }
        public virtual ICollection<TreatyLayer> Layers { get; set; } = new List<TreatyLayer>();
    }

    public class TreatyLayer : BaseEntity
    {
        public int TreatyId { get; set; }
        public int LayerNumber { get; set; }
        public decimal Limit { get; set; }
        public decimal Attachment { get; set; }
        public int Reinstatements { get; set; }
        public decimal ReinstatementPremiumPct { get; set; }
          public decimal SharePct { get; set; }
          public string Currency { get; set; }
          public virtual Treaty Treaty { get; set; }
      }

    public class ExposureRecord : BaseEntity
    {
        public int SubmissionId { get; set; }
        public int RegionId { get; set; }
        public int PerilId { get; set; }
        public decimal TotalInsuredValue { get; set; }
        public int RiskCount { get; set; }
        public decimal AverageDeductiblePct { get; set; }
        public virtual Submission Submission { get; set; }
        public virtual Region Region { get; set; }
        public virtual Peril Peril { get; set; }
    }

    public class LossEvent : BaseEntity
    {
        public int CedentId { get; set; }
        public int RegionId { get; set; }
        public int PerilId { get; set; }
        public string EventName { get; set; }
        public DateTime LossDate { get; set; }
        public decimal GroundUpLoss { get; set; }
        public decimal? CededLoss { get; set; }
        public int? TreatyId { get; set; }
    }

    public class CatModelResult : BaseEntity
    {
        public int SubmissionId { get; set; }
        public int? RegionId { get; set; }
        public int? PerilId { get; set; }
        public string ModelVendor { get; set; }
        public string ModelVersion { get; set; }
        public decimal AAL { get; set; }
        public decimal ExpectedLoss { get; set; }
        public decimal PML50 { get; set; }
        public decimal PML100 { get; set; }
        public decimal PML250 { get; set; }
        public DateTime RunOn { get; set; }
        public virtual Submission Submission { get; set; }
    }

    public class PricingResult : BaseEntity, IAuditable
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
        public DateTime CalculatedOn { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public virtual TreatyLayer TreatyLayer { get; set; }
    }

    public class PortfolioLimit : BaseEntity
    {
        public int? RegionId { get; set; }
        public int? PerilId { get; set; }
        public decimal MaxAggregateLimit { get; set; }
        public decimal MaxPML250 { get; set; }
        public string Description { get; set; }
    }

    public class ReferralRule : BaseEntity
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public ReferralRuleKind Kind { get; set; }
        public decimal? Threshold { get; set; }
        public string TextValue { get; set; }
    }
}
