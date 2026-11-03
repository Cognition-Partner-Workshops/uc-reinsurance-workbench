using System;
using NUnit.Framework;
using Reinsurance.Services.Pricing;
using Sentry;
using Reinsurance.Tests.Fakes;

namespace Reinsurance.Tests.Incidents
{
    /// <summary>
    /// Regression coverage for the Atlas boundary case (layer attachment equal to the portfolio PML250),
    /// which previously threw DivideByZeroException from PricingCalculator.LayerHitFraction.
    /// </summary>
    [TestFixture]
    [Category("PlantedIncident")]
    public class AtlasPricingIncidentTests
    {
        private FakeTransport _transport;

        [TearDown]
        public void TearDown()
        {
            SentrySdk.Close();
        }

        [Test]
        public void AttachmentEqualToPml250ReturnsFiniteHitFraction()
        {
            var hitFraction = PricingCalculator.LayerHitFraction(25000000m, 300000000m, 300000000m);

            Assert.That(hitFraction, Is.EqualTo(0m));
        }

        [Test]
        public void AttachmentEqualToPml250PricesWithoutThrowing()
        {
            PricingOutput result = null;
            Assert.DoesNotThrow(() => result = PricingCalculator.Calculate(new PricingInput
            {
                Limit = 25000000m,
                Attachment = 300000000m,
                PML250 = 300000000m,
                AAL = 9000000m
            }));

            Assert.That(result.ExpectedLoss, Is.EqualTo(0m));
            Assert.That(result.TechnicalPremium, Is.EqualTo(0m));
        }

        [Test]
        public void SentryCapturesNothingForAtlasBoundaryCalculation()
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
                PricingCalculator.Calculate(new PricingInput
                {
                    Limit = 25000000m,
                    Attachment = 300000000m,
                    PML250 = 300000000m,
                    AAL = 9000000m
                });
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
