using System;
using NUnit.Framework;
using Reinsurance.Services.Pricing;
using Sentry;
using Reinsurance.Tests.Fakes;

namespace Reinsurance.Tests.Incidents
{
    /// <summary>
    /// Pins the current Atlas divide-by-zero failure mode; when fixed, flip this to assert a finite hit fraction.
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
        public void AttachmentEqualToPml250ThrowsDivideByZero()
        {
            Assert.Throws<DivideByZeroException>(() => PricingCalculator.Calculate(new PricingInput
            {
                Limit = 25000000m,
                Attachment = 300000000m,
                PML250 = 300000000m,
                AAL = 9000000m
            }));
        }

        [Test]
        public void SentryCapturesAtlasCalculationFailure()
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
            Assert.That(_transport.Envelopes, Has.Count.EqualTo(1));
        }

    }
}
