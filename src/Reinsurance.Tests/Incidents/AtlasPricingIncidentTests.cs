using System;
using NUnit.Framework;
using Reinsurance.Services.Pricing;
using Sentry;
using Reinsurance.Tests.Fakes;

namespace Reinsurance.Tests.Incidents
{
    /// <summary>
    /// Regression coverage for the Atlas layer-3 pricing incident (attachment equal to portfolio PML250).
    /// </summary>
    [TestFixture]
    [Category("PlantedIncident")]
    public class AtlasPricingIncidentTests
    {
        private static readonly PricingInput AtlasLayerThree = new PricingInput
        {
            Limit = 25000000m,
            Attachment = 300000000m,
            PML250 = 300000000m,
            AAL = 9000000m,
            SharePct = 1m,
            Reinstatements = 1,
            ReinstatementPremiumPct = 0.5m
        };

        private FakeTransport _transport;

        [TearDown]
        public void TearDown()
        {
            SentrySdk.Close();
        }

        [Test]
        public void AttachmentEqualToPml250ProducesFiniteHitFraction()
        {
            var fraction = PricingCalculator.LayerHitFraction(AtlasLayerThree.Limit, AtlasLayerThree.Attachment, AtlasLayerThree.PML250);

            Assert.That(fraction, Is.EqualTo(0m));
        }

        [Test]
        public void AttachmentEqualToPml250PricesWithoutThrowing()
        {
            PricingOutput result = null;
            Assert.DoesNotThrow(() => result = PricingCalculator.Calculate(AtlasLayerThree));

            Assert.That(result.ExpectedLoss, Is.EqualTo(0m));
            Assert.That(result.RiskLoad, Is.EqualTo(0m));
            Assert.That(result.ExpenseLoad, Is.EqualTo(0m));
            Assert.That(result.TechnicalPremium, Is.EqualTo(0m));
            Assert.That(result.OurShare, Is.EqualTo(0m));
            Assert.That(result.RateOnLine, Is.EqualTo(0m));
            Assert.That(result.LossCostPct, Is.EqualTo(0m));
        }

        [Test]
        public void SentryDoesNotCaptureAtlasCalculation()
        {
            _transport = new FakeTransport();
            SentrySdk.Init(options =>
            {
                options.Dsn = "https://public@sentry.invalid/1";
                options.Transport = _transport;
                options.FlushTimeout = TimeSpan.FromSeconds(5);
            });

            Assert.That(SentrySdk.IsEnabled, Is.True);
            try
            {
                PricingCalculator.Calculate(AtlasLayerThree);
            }
            catch (DivideByZeroException exception)
            {
                SentrySdk.CaptureException(exception);
            }

            SentrySdk.Close();
            Assert.That(_transport.Envelopes, Is.Empty);
        }
    }
}
