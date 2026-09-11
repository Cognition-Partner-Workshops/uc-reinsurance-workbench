using System;
using NUnit.Framework;
using Reinsurance.Services.Pricing;
using Sentry;
using Reinsurance.Tests.Fakes;

namespace Reinsurance.Tests.Incidents
{
    /// <summary>
    /// Regression coverage for the Atlas layer whose attachment equals the portfolio PML250 (Sentry BACKEND-REINSURANCE-DEMO-1).
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
            AAL = 9000000m
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
            Assert.That(PricingCalculator.LayerHitFraction(AtlasLayerThree.Limit, AtlasLayerThree.Attachment, AtlasLayerThree.PML250), Is.EqualTo(0m));
        }

        [Test]
        public void AttachmentEqualToPml250PricesWithoutThrowing()
        {
            PricingOutput result = null;
            Assert.DoesNotThrow(() => result = PricingCalculator.Calculate(AtlasLayerThree));

            Assert.That(result.ExpectedLoss, Is.EqualTo(0m));
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
            catch (Exception exception)
            {
                SentrySdk.CaptureException(exception);
            }

            SentrySdk.Close();
            Assert.That(_transport.Envelopes, Is.Empty);
        }
    }
}
