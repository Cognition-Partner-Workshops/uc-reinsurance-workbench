using System;
using System.Collections.Generic;
using NUnit.Framework;
using Reinsurance.Core.Domain.Cedents;
using Reinsurance.Core.Domain.Exposure;
using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Referrals;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Domain.Treaties;
using Reinsurance.Services.Referrals;

namespace Reinsurance.Tests.Referrals
{
    [TestFixture]
    public class ReferralEngineTests
    {
        [Test]
        [Category("Unit")]
        public void MaxLayerLimitTriggersAndDoesNotTrigger()
        {
            var rule = Rule(ReferralRuleKind.MaxLayerLimit, threshold: 100m);
            Assert.That(Evaluate(rule, layerLimit: 101m).Required, Is.True);
            Assert.That(Evaluate(rule, layerLimit: 100m).Required, Is.False);
        }

        [Test]
        [Category("Unit")]
        public void MinRateOnLineTriggersAndDoesNotTrigger()
        {
            var rule = Rule(ReferralRuleKind.MinRateOnLine, threshold: 0.02m);
            Assert.That(Evaluate(rule, rateOnLine: 0.019m).Required, Is.True);
            Assert.That(Evaluate(rule, rateOnLine: 0.02m).Required, Is.False);
        }

        [Test]
        [Category("Unit")]
        public void MaxRateOnLineTriggersAndDoesNotTrigger()
        {
            var rule = Rule(ReferralRuleKind.MaxRateOnLine, threshold: 0.35m);
            Assert.That(Evaluate(rule, rateOnLine: 0.351m).Required, Is.True);
            Assert.That(Evaluate(rule, rateOnLine: 0.35m).Required, Is.False);
        }

        [Test]
        [Category("Unit")]
        public void CedentRatingBelowTriggersAndDoesNotTrigger()
        {
            var rule = Rule(ReferralRuleKind.CedentRatingBelow, textValue: "A-");
            Assert.That(Evaluate(rule, rating: "B+").Required, Is.True);
            Assert.That(Evaluate(rule, rating: "A-").Required, Is.False);
        }

        [Test]
        [Category("Unit")]
        public void UnderwriterAuthorityTriggersAndDoesNotTrigger()
        {
            var rule = Rule(ReferralRuleKind.UnderwriterAuthority);
            Assert.That(Evaluate(rule, layerLimit: 101m, authority: 100m).Required, Is.True);
            Assert.That(Evaluate(rule, layerLimit: 100m, authority: 100m).Required, Is.False);
        }

        [Test]
        [Category("Unit")]
        public void PortfolioLimitBreachTriggersAndDoesNotTrigger()
        {
            var rule = Rule(ReferralRuleKind.PortfolioLimitBreach);
            var limit = new PortfolioLimit { RegionId = 7, MaxAggregateLimit = 150m, Description = "Southeast" };
            var layer = Layer(100m);
            var submission = Submission(7, 9);
            var engine = new ReferralEngine();
            var breach = engine.Evaluate(layer, Pricing(0.1m), submission, new Underwriter { AuthorityLimit = 1000m },
                new[] { rule }, new[] { limit }, x => 51m, 0m);
            var clear = engine.Evaluate(layer, Pricing(0.1m), submission, new Underwriter { AuthorityLimit = 1000m },
                new[] { rule }, new[] { limit }, x => 50m, 0m);

            Assert.That(breach.Required, Is.True);
            Assert.That(breach.Reasons[0], Does.Contain("aggregate 151 > limit 150"));
            Assert.That(clear.Required, Is.False);
        }

        [Test]
        [Category("Unit")]
        public void RatingScaleUsesAmBestOrdering()
        {
            Assert.That(Evaluate(Rule(ReferralRuleKind.CedentRatingBelow, textValue: "A-"), rating: "A++").Required, Is.False);
            Assert.That(Evaluate(Rule(ReferralRuleKind.CedentRatingBelow, textValue: "A-"), rating: "A+").Required, Is.False);
            Assert.That(Evaluate(Rule(ReferralRuleKind.CedentRatingBelow, textValue: "A-"), rating: "A").Required, Is.False);
            Assert.That(Evaluate(Rule(ReferralRuleKind.CedentRatingBelow, textValue: "A-"), rating: "A-").Required, Is.False);
            Assert.That(Evaluate(Rule(ReferralRuleKind.CedentRatingBelow, textValue: "A-"), rating: "B++").Required, Is.True);
            Assert.That(Evaluate(Rule(ReferralRuleKind.CedentRatingBelow, textValue: "B++"), rating: "B+").Required, Is.True);
            Assert.That(Evaluate(Rule(ReferralRuleKind.CedentRatingBelow, textValue: "A-"), rating: "UNKNOWN").Required, Is.True);
        }

        [Test]
        [Category("Unit")]
        public void InactiveRulesAreIgnored()
        {
            var rule = Rule(ReferralRuleKind.MaxLayerLimit, threshold: 1m);
            rule.Active = false;

            Assert.That(Evaluate(rule, layerLimit: 100m).Required, Is.False);
        }

        [Test]
        [Category("Unit")]
        public void PortfolioLimitMatchesRegionOnlyPerilOnlyBothAndWildcard()
        {
            var regionIds = new HashSet<int> { 7 };
            var perilIds = new HashSet<int> { 9 };

            Assert.That(ReferralEngine.Matches(new PortfolioLimit { RegionId = 7 }, regionIds, perilIds), Is.True);
            Assert.That(ReferralEngine.Matches(new PortfolioLimit { RegionId = 8 }, regionIds, perilIds), Is.False);
            Assert.That(ReferralEngine.Matches(new PortfolioLimit { PerilId = 9 }, regionIds, perilIds), Is.True);
            Assert.That(ReferralEngine.Matches(new PortfolioLimit { PerilId = 10 }, regionIds, perilIds), Is.False);
            Assert.That(ReferralEngine.Matches(new PortfolioLimit { RegionId = 7, PerilId = 9 }, regionIds, perilIds), Is.True);
            Assert.That(ReferralEngine.Matches(new PortfolioLimit { RegionId = 7, PerilId = 10 }, regionIds, perilIds), Is.False);
            Assert.That(ReferralEngine.Matches(new PortfolioLimit(), regionIds, perilIds), Is.True);
        }

        private static ReferralDecision Evaluate(
            ReferralRule rule,
            decimal layerLimit = 50m,
            decimal rateOnLine = 0.1m,
            string rating = "A",
            decimal authority = 1000m)
        {
            return new ReferralEngine().Evaluate(
                Layer(layerLimit),
                Pricing(rateOnLine),
                Submission(1, 1, rating),
                new Underwriter { AuthorityLimit = authority },
                new[] { rule },
                new List<PortfolioLimit>(),
                x => 0m,
                0m);
        }

        private static ReferralRule Rule(ReferralRuleKind kind, decimal? threshold = null, string textValue = null)
        {
            return new ReferralRule
            {
                Code = "TEST",
                Description = "Test referral",
                Active = true,
                Kind = kind,
                Threshold = threshold,
                TextValue = textValue
            };
        }

        private static TreatyLayer Layer(decimal limit)
        {
            return new TreatyLayer { Id = 1, Limit = limit };
        }

        private static PricingResult Pricing(decimal rateOnLine)
        {
            return new PricingResult { RateOnLine = rateOnLine };
        }

        private static Submission Submission(int regionId, int perilId, string rating = "A")
        {
            return new Submission
            {
                Id = 1,
                Cedent = new Cedent { Rating = rating },
                ExposureRecords = new List<ExposureRecord>
                {
                    new ExposureRecord { RegionId = regionId, PerilId = perilId }
                }
            };
        }
    }
}
