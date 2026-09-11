using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Domain;

namespace Reinsurance.Services.Referrals
{
    public sealed class ReferralDecision
    {
        public bool Required { get; set; }
        public IList<string> Reasons { get; set; } = new List<string>();
    }

    public sealed class ReferralEngine
    {
        public ReferralDecision Evaluate(TreatyLayer layer, PricingResult pricing, Underwriter underwriter, IEnumerable<ReferralRule> rules)
        {
            var decision = new ReferralDecision();
            foreach (var rule in rules.Where(x => x.Active))
            {
                if (rule.Kind == ReferralRuleKind.MaxLayerLimit && rule.Threshold.HasValue && layer.Limit > rule.Threshold.Value)
                    decision.Reasons.Add(rule.Description);
                if (rule.Kind == ReferralRuleKind.MinRateOnLine && rule.Threshold.HasValue && pricing.RateOnLine < rule.Threshold.Value)
                    decision.Reasons.Add(rule.Description);
                if (rule.Kind == ReferralRuleKind.MaxRateOnLine && rule.Threshold.HasValue && pricing.RateOnLine > rule.Threshold.Value)
                    decision.Reasons.Add(rule.Description);
                if (rule.Kind == ReferralRuleKind.UnderwriterAuthority && underwriter != null && layer.Limit > underwriter.AuthorityLimit)
                    decision.Reasons.Add(rule.Description);
            }
            decision.Required = decision.Reasons.Count > 0;
            return decision;
        }
    }
}
