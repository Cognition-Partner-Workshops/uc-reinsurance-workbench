using System;
using NUnit.Framework;
using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Domain.Treaties;
using Reinsurance.Services.Submissions;
using Reinsurance.Tests.Fakes;

namespace Reinsurance.Tests.Incidents
{
    /// <summary>
    /// Regression coverage for the quoted-submission decline transition failure
    /// (Sentry TRADER-DEMO-APP-D: Max over an empty pricing-result set threw InvalidOperationException).
    /// </summary>
    [TestFixture]
    [Category("PlantedIncident")]
    public class DeclineTransitionIncidentTests
    {
        [Test]
        public void DecliningQuotedSubmissionWithoutPricingTransitionsToDeclined()
        {
            var entity = QuotedSubmissionWithLayer(layerId: 10);
            var service = Service(entity, new FakeRepository<PricingResult>());

            var result = service.Transition(1, SubmissionStatus.Declined);

            Assert.That(result.Status, Is.EqualTo(SubmissionStatus.Declined));
            Assert.That(entity.Status, Is.EqualTo(SubmissionStatus.Declined));
            Assert.That(entity.Notes, Does.Contain("Declined " + DateTime.UtcNow.ToString("yyyy-MM-dd")));
            Assert.That(entity.Notes, Does.Contain("no quote on record"));
        }

        [Test]
        public void DecliningQuotedSubmissionWithPricingRecordsLatestQuoteDate()
        {
            var entity = QuotedSubmissionWithLayer(layerId: 10);
            var pricing = new FakeRepository<PricingResult>(new[]
            {
                new PricingResult { Id = 1, TreatyLayerId = 10, CalculatedOn = new DateTime(2026, 2, 1) },
                new PricingResult { Id = 2, TreatyLayerId = 10, CalculatedOn = new DateTime(2026, 3, 15) },
                new PricingResult { Id = 3, TreatyLayerId = 99, CalculatedOn = new DateTime(2026, 9, 9) }
            });
            var service = Service(entity, pricing);

            var result = service.Transition(1, SubmissionStatus.Declined);

            Assert.That(result.Status, Is.EqualTo(SubmissionStatus.Declined));
            Assert.That(entity.Notes, Does.Contain("last quote 2026-03-15"));
            Assert.That(entity.Notes, Does.Not.Contain("2026-09-09"));
        }

        [Test]
        [Category("Unit")]
        public void QuotedSubmissionCanBind()
        {
            var service = Service(
                new Submission { Id = 1, Status = SubmissionStatus.Quoted },
                new FakeRepository<PricingResult>());

            Assert.That(service.Transition(1, SubmissionStatus.Bound).Status, Is.EqualTo(SubmissionStatus.Bound));
        }

        private static Submission QuotedSubmissionWithLayer(int layerId)
        {
            var entity = new Submission { Id = 1, Status = SubmissionStatus.Quoted, Notes = "Placement notes." };
            var treaty = new Treaty { Id = 1, SubmissionId = 1 };
            treaty.Layers.Add(new TreatyLayer { Id = layerId, TreatyId = 1, LayerNumber = 1 });
            entity.Treaties.Add(treaty);
            return entity;
        }

        private static SubmissionService Service(Submission entity, FakeRepository<PricingResult> pricing)
        {
            return new SubmissionService(
                new FakeRepository<Submission>(new[] { entity }),
                new EmptyExposureService(),
                pricing);
        }
    }
}
