using NUnit.Framework;
using Reinsurance.Services.Pricing;

namespace Reinsurance.Tests.Pricing
{
    [TestFixture]
    public class PricingCalculatorTests
    {
        [Test]
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
        public void PlantedIncidentCurrentlyThrowsFromUngardedDenominator()
        {
            Assert.Throws<System.DivideByZeroException>(() => PricingCalculator.Calculate(new PricingInput
            {
                Limit = 25000000m,
                Attachment = 100000000m,
                AAL = 8500000m,
                PML100 = 65000000m,
                PML250 = 100000000m
            }));
        }
    }
}
