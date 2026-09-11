using System;
using Reinsurance.Core.Domain.Referrals;

namespace Reinsurance.Services.Referrals.Models
{
    public sealed class ReferralRuleModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public ReferralRuleKind Kind { get; set; }
        public decimal? Threshold { get; set; }
        public string TextValue { get; set; }
    }
}
