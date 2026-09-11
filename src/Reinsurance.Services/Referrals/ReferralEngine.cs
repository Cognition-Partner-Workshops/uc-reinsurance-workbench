using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Domain.Cedents;
using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Referrals;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Domain.Treaties;

namespace Reinsurance.Services.Referrals
{
    public sealed class ReferralDecision
    {
        public bool Required { get; set; }
        public IList<string> Reasons { get; set; } = new List<string>();
    }

    public sealed class ReferralEngine
    {
        private static readonly string[] RatingScale = { "A++", "A+", "A", "A-", "B++", "B+", "B", "B-", "C" };

        public ReferralDecision Evaluate(TreatyLayer layer, PricingResult pricing, Submission submission, Underwriter underwriter, IEnumerable<ReferralRule> rules, IEnumerable<PortfolioLimit> limits, IEnumerable<TreatyLayer> boundLayers)
        {
            var decision = new ReferralDecision();
            foreach (var rule in rules.Where(x => x.Active))
            {
                var triggered = false;
                switch (rule.Kind)
                {
                    case ReferralRuleKind.MaxLayerLimit:
                        triggered = rule.Threshold.HasValue && layer.Limit > rule.Threshold.Value;
                        break;
                    case ReferralRuleKind.MinRateOnLine:
                        triggered = rule.Threshold.HasValue && pricing.RateOnLine < rule.Threshold.Value;
                        break;
                    case ReferralRuleKind.MaxRateOnLine:
                        triggered = rule.Threshold.HasValue && pricing.RateOnLine > rule.Threshold.Value;
                        break;
                    case ReferralRuleKind.CedentRatingBelow:
                        triggered = IsBelow(submission.Cedent == null ? null : submission.Cedent.Rating, rule.TextValue);
                        break;
                    case ReferralRuleKind.UnderwriterAuthority:
                        triggered = underwriter == null || layer.Limit > underwriter.AuthorityLimit;
                        break;
                    case ReferralRuleKind.PortfolioLimitBreach:
                        triggered = limits.Any(x => boundLayers.Sum(y => y.Limit) + layer.Limit > x.MaxAggregateLimit);
                        break;
                }
                if (triggered)
                    decision.Reasons.Add(rule.Code + ": " + rule.Description);
            }
            decision.Required = decision.Reasons.Count > 0;
            return decision;
        }

        private static bool IsBelow(string actual, string threshold)
        {
            var actualIndex = RatingScale.ToList().IndexOf(actual ?? "C");
            var thresholdIndex = RatingScale.ToList().IndexOf(threshold ?? "C");
            return actualIndex >= 0 && thresholdIndex >= 0 && actualIndex > thresholdIndex;
        }
    }
}
