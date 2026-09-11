using System;
using System.Collections.Generic;
using Reinsurance.Core.Domain;

namespace Reinsurance.Services.DTOs
{
    public class CedentDto { public int Id { get; set; } public string Name { get; set; } public string Code { get; set; } public string Country { get; set; } public string Rating { get; set; } public bool Active { get; set; } }
    public class SubmissionDto { public int Id { get; set; } public string Reference { get; set; } public string Cedent { get; set; } public string Broker { get; set; } public string Underwriter { get; set; } public SubmissionStatus Status { get; set; } public DateTime InceptionDate { get; set; } public DateTime ExpiryDate { get; set; } public string Notes { get; set; } }
    public class SubmissionCreateDto { public string Reference { get; set; } public int CedentId { get; set; } public int? BrokerId { get; set; } public int? UnderwriterId { get; set; } public DateTime InceptionDate { get; set; } public DateTime ExpiryDate { get; set; } public string Notes { get; set; } }
    public class TransitionDto { public SubmissionStatus Status { get; set; } }
    public class ExposureDto { public int Id { get; set; } public string Region { get; set; } public string Peril { get; set; } public decimal TotalInsuredValue { get; set; } public int RiskCount { get; set; } public decimal AverageDeductiblePct { get; set; } }
    public class CatModelDto { public int Id { get; set; } public string ModelVendor { get; set; } public string ModelVersion { get; set; } public decimal AAL { get; set; } public decimal ExpectedLoss { get; set; } public decimal PML50 { get; set; } public decimal PML100 { get; set; } public decimal PML250 { get; set; } public DateTime RunOn { get; set; } public int? RegionId { get; set; } public int? PerilId { get; set; } }
    public class TreatyCreateDto { public int SubmissionId { get; set; } public string Name { get; set; } public TreatyType Type { get; set; } public string Currency { get; set; } public DateTime InceptionDate { get; set; } public DateTime ExpiryDate { get; set; } }
    public class LayerDto { public int Id { get; set; } public int LayerNumber { get; set; } public decimal Limit { get; set; } public decimal Attachment { get; set; } public int Reinstatements { get; set; } public decimal ReinstatementPremiumPct { get; set; } public decimal SharePct { get; set; } public string Currency { get; set; } }
    public class TreatyDto { public int Id { get; set; } public string Name { get; set; } public TreatyType Type { get; set; } public TreatyStatus Status { get; set; } public string Currency { get; set; } public int SubmissionId { get; set; } public IList<LayerDto> Layers { get; set; } }
    public class PricingResultDto { public int TreatyLayerId { get; set; } public decimal TechnicalPremium { get; set; } public decimal ExpectedLoss { get; set; } public decimal ExpenseLoad { get; set; } public decimal RiskLoad { get; set; } public decimal RateOnLine { get; set; } public decimal LossCostPct { get; set; } public decimal ProfitMarginPct { get; set; } public bool ReferralRequired { get; set; } public string ReferralReasons { get; set; } public decimal OurShareLine { get; set; } }
    public class ReferralRuleDto { public int Id { get; set; } public string Code { get; set; } public string Description { get; set; } public bool Active { get; set; } public ReferralRuleKind Kind { get; set; } public decimal? Threshold { get; set; } public string TextValue { get; set; } }
    public class LossHistoryDto { public int Id { get; set; } public int CedentId { get; set; } public string EventName { get; set; } public DateTime LossDate { get; set; } public decimal GroundUpLoss { get; set; } public decimal? CededLoss { get; set; } }
}
