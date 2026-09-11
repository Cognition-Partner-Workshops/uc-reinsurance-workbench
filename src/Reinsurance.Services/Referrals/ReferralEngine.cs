using System;
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

        public ReferralDecision Evaluate(
            TreatyLayer layer,
            PricingResult pricing,
            Submission submission,
            Underwriter underwriter,
            IEnumerable<ReferralRule> rules,
            IEnumerable<PortfolioLimit> limits,
            Func<PortfolioLimit, decimal> boundAggregateFor,
            decimal latestPortfolioPml250,
            Func<int?, string> regionCodeFor = null,
            Func<int?, string> perilCodeFor = null)
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
                        var breach = limits
                            .Where(x => Matches(
                                x,
                                new HashSet<int>(submission.ExposureRecords.Select(y => y.RegionId)),
                                new HashSet<int>(submission.ExposureRecords.Select(y => y.PerilId))))
                            .Select(x => new
                            {
                                Limit = x,
                                Aggregate = boundAggregateFor(x) + layer.Limit,
                                PmlBreach = x.MaxPML250 > 0m && latestPortfolioPml250 > x.MaxPML250
                            })
                            .FirstOrDefault(x => x.Aggregate > x.Limit.MaxAggregateLimit || x.PmlBreach);
                        if (breach != null)
                        {
                            var regionCode = breach.Limit.RegionId.HasValue
                                ? (regionCodeFor == null ? breach.Limit.RegionId.Value.ToString() : regionCodeFor(breach.Limit.RegionId))
                                : "*";
                            var perilCode = breach.Limit.PerilId.HasValue
                                ? (perilCodeFor == null ? breach.Limit.PerilId.Value.ToString() : perilCodeFor(breach.Limit.PerilId))
                                : "*";
                            if (breach.Aggregate > breach.Limit.MaxAggregateLimit)
                            {
                                decision.Reasons.Add(string.Format(
                                    "{0}: {1} (aggregate {2:N0} > limit {3:N0} for {4}/{5})",
                                    rule.Code,
                                    rule.Description,
                                    breach.Aggregate,
                                    breach.Limit.MaxAggregateLimit,
                                    regionCode,
                                    perilCode));
                            }
                            else
                            {
                                decision.Reasons.Add(string.Format(
                                    "{0}: {1} (PML250 {2:N0} > limit {3:N0} for {4}/{5})",
                                    rule.Code,
                                    rule.Description,
                                    latestPortfolioPml250,
                                    breach.Limit.MaxPML250,
                                    regionCode,
                                    perilCode));
                            }
                            continue;
                        }
                        break;
                }
                if (triggered)
                    decision.Reasons.Add(rule.Code + ": " + rule.Description);
            }
            decision.Required = decision.Reasons.Count > 0;
            return decision;
        }

        public static bool Matches(PortfolioLimit limit, ISet<int> regionIds, ISet<int> perilIds)
        {
            return (limit.RegionId == null || regionIds.Contains(limit.RegionId.Value))
                && (limit.PerilId == null || perilIds.Contains(limit.PerilId.Value));
        }

        private static bool IsBelow(string actual, string threshold)
        {
            var actualIndex = RatingScale.ToList().IndexOf(actual);
            var thresholdIndex = RatingScale.ToList().IndexOf(threshold);
            if (actualIndex < 0)
                actualIndex = RatingScale.Length - 1;
            if (thresholdIndex < 0)
                thresholdIndex = RatingScale.Length - 1;
            return actualIndex >= 0 && thresholdIndex >= 0 && actualIndex > thresholdIndex;
        }
    }
}
