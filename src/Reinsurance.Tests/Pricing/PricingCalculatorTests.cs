using NUnit.Framework;
using Reinsurance.Services.Pricing;

namespace Reinsurance.Tests.Pricing
{
    [TestFixture]
    public class PricingCalculatorTests
    {
        [Test]
        [Category("Unit")]
        public void GoldenValuesForFiftyMillionXsFiftyMillionLayer()
        {
            var result = PricingCalculator.Calculate(new PricingInput
            {
                Limit = 50000000m,
                Attachment = 50000000m,
                AAL = 12000000m,
                PML100 = 180000000m,
                PML250 = 300000000m,
                SharePct = 1m,
                Reinstatements = 0
            });

            Assert.That(result.ExpectedLoss, Is.EqualTo(2000000m));
            Assert.That(result.RiskLoad, Is.EqualTo(1500000m));
            Assert.That(result.ExpenseLoad, Is.EqualTo(420000m));
            Assert.That(result.TechnicalPremium, Is.EqualTo(4260869.57m));
            Assert.That(result.RateOnLine, Is.EqualTo(0.085217m));
            Assert.That(result.LossCostPct, Is.EqualTo(0.04m));
        }

        [Test]
        [Category("Unit")]
        public void HealthyLayerProducesFinitePremium()
        {
            var result = PricingCalculator.Calculate(new PricingInput
            {
                Limit = 25000000m,
                Attachment = 25000000m,
                AAL = 12000000m,
                PML100 = 195000000m,
                PML250 = 300000000m,
                SharePct = 1m,
                Reinstatements = 1,
                ReinstatementPremiumPct = 0.5m
            });
            Assert.That(result.TechnicalPremium, Is.GreaterThan(0m));
            Assert.That(result.RateOnLine, Is.GreaterThan(0m));
        }

        [Test]
        [Category("Unit")]
        public void AttachmentEqualToPml250PricesToZeroWithoutThrowing()
        {
            var result = PricingCalculator.Calculate(new PricingInput
            {
                Limit = 25000000m,
                Attachment = 100000000m,
                AAL = 8500000m,
                PML100 = 65000000m,
                PML250 = 100000000m
            });

            Assert.That(result.ExpectedLoss, Is.EqualTo(0m));
            Assert.That(result.TechnicalPremium, Is.EqualTo(0m));
            Assert.That(result.RateOnLine, Is.EqualTo(0m));
        }

        [Test]
        [Category("Unit")]
        public void AttachmentAbovePml250PricesToZeroWithoutThrowing()
        {
            var result = PricingCalculator.Calculate(new PricingInput
            {
                Limit = 25000000m,
                Attachment = 120000000m,
                AAL = 8500000m,
                PML250 = 100000000m
            });

            Assert.That(result.ExpectedLoss, Is.EqualTo(0m));
            Assert.That(result.TechnicalPremium, Is.EqualTo(0m));
        }

        [Test]
        [Category("Unit")]
        public void HigherAttachmentReducesExpectedLossAndRateOnLine()
        {
            var lower = PricingCalculator.Calculate(new PricingInput
            {
                Limit = 50000000m,
                Attachment = 200000000m,
                AAL = 12000000m,
                PML250 = 300000000m,
                SharePct = 1m
            });
            var higher = PricingCalculator.Calculate(new PricingInput
            {
                Limit = 50000000m,
                Attachment = 260000000m,
                AAL = 12000000m,
                PML250 = 300000000m,
                SharePct = 1m
            });

            Assert.That(higher.ExpectedLoss, Is.LessThan(lower.ExpectedLoss));
            Assert.That(higher.RateOnLine, Is.LessThan(lower.RateOnLine));
        }

        [Test]
        [Category("Unit")]
        public void ZeroPmlReturnsExpectedLossAndPremiumFloor()
        {
            var result = PricingCalculator.Calculate(new PricingInput { Limit = 50000000m, PML250 = 0m });

            Assert.That(result.ExpectedLoss, Is.EqualTo(0m));
            Assert.That(result.TechnicalPremium, Is.EqualTo(result.RiskLoad + result.ExpenseLoad));
        }

        [Test]
        [Category("Unit")]
        public void ReinstatementsIncreaseTechnicalPremium()
        {
            var without = PricingCalculator.Calculate(new PricingInput
            {
                Limit = 50000000m, Attachment = 50000000m, AAL = 12000000m, PML250 = 300000000m,
                ReinstatementPremiumPct = 0.5m
            });
            var with = PricingCalculator.Calculate(new PricingInput
            {
                Limit = 50000000m, Attachment = 50000000m, AAL = 12000000m, PML250 = 300000000m,
                Reinstatements = 2, ReinstatementPremiumPct = 0.5m
            });

            Assert.That(with.TechnicalPremium, Is.GreaterThan(without.TechnicalPremium));
        }

        [Test]
        [Category("Unit")]
        public void SharePctScalesOurShareOnly()
        {
            var full = PricingCalculator.Calculate(new PricingInput
            {
                Limit = 50000000m, Attachment = 50000000m, AAL = 12000000m, PML250 = 300000000m,
                SharePct = 1m
            });
            var half = PricingCalculator.Calculate(new PricingInput
            {
                Limit = 50000000m, Attachment = 50000000m, AAL = 12000000m, PML250 = 300000000m,
                SharePct = 0.5m
            });

            Assert.That(half.OurShare, Is.EqualTo(2130434.79m));
            Assert.That(half.TechnicalPremium, Is.EqualTo(full.TechnicalPremium));
            Assert.That(half.ExpectedLoss, Is.EqualTo(full.ExpectedLoss));
            Assert.That(half.RiskLoad, Is.EqualTo(full.RiskLoad));
        }
    }
}
