using System;
using NUnit.Framework;
using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Domain.Treaties;
using Reinsurance.Services.Exposure;
using Reinsurance.Services.Submissions;
using Reinsurance.Services.Submissions.Models;
using Reinsurance.Tests.Fakes;

namespace Reinsurance.Tests.Incidents
{
    /// <summary>
    /// Regression coverage for declining a quoted submission whose treaty layers have no pricing results (Sentry BACKEND-REINSURANCE-DEMO-3).
    /// </summary>
    [TestFixture]
    [Category("PlantedIncident")]
    public class DeclineTransitionIncidentTests
    {
        [Test]
        public void DecliningQuotedSubmissionWithoutPricingTransitionsToDeclined()
        {
            var entity = QuotedSubmissionWithLayer(notes: "Broker follow-up pending.");
            var service = new SubmissionService(
                new FakeRepository<Submission>(new[] { entity }),
                new EmptyExposureService(),
                new FakeRepository<PricingResult>());

            AssertDeclined(service.Transition(1, SubmissionStatus.Declined), entity);
            Assert.That(entity.Notes, Does.StartWith("Broker follow-up pending. Declined "));
            Assert.That(entity.Notes, Does.EndWith("; no quote on record."));
        }

        [Test]
        public void DecliningQuotedSubmissionWithPricingRecordsLatestQuoteDate()
        {
            var entity = QuotedSubmissionWithLayer(notes: null);
            var service = new SubmissionService(
                new FakeRepository<Submission>(new[] { entity }),
                new EmptyExposureService(),
                new FakeRepository<PricingResult>(new[]
                {
                    new PricingResult { Id = 1, TreatyLayerId = 10, CalculatedOn = new DateTime(2026, 3, 1) },
                    new PricingResult { Id = 2, TreatyLayerId = 10, CalculatedOn = new DateTime(2026, 4, 15) },
                    new PricingResult { Id = 3, TreatyLayerId = 99, CalculatedOn = new DateTime(2026, 8, 1) }
                }));

            AssertDeclined(service.Transition(1, SubmissionStatus.Declined), entity);
            Assert.That(entity.Notes, Does.StartWith("Declined "));
            Assert.That(entity.Notes, Does.EndWith("; last quote 2026-04-15."));
        }

        [Test]
        [Category("Unit")]
        public void QuotedSubmissionCanBind()
        {
            var service = new SubmissionService(
                new FakeRepository<Submission>(new[]
                {
                    new Submission { Id = 1, Status = SubmissionStatus.Quoted }
                }),
                new EmptyExposureService(),
                new FakeRepository<PricingResult>());

            Assert.That(service.Transition(1, SubmissionStatus.Bound).Status, Is.EqualTo(SubmissionStatus.Bound));
        }

        private static Submission QuotedSubmissionWithLayer(string notes)
        {
            var layer = new TreatyLayer { Id = 10, LayerNumber = 1 };
            var treaty = new Treaty { Id = 5, SubmissionId = 1 };
            treaty.Layers.Add(layer);
            var entity = new Submission { Id = 1, Status = SubmissionStatus.Quoted, Notes = notes };
            entity.Treaties.Add(treaty);
            return entity;
        }

        private static void AssertDeclined(SubmissionModel model, Submission entity)
        {
            Assert.That(model.Status, Is.EqualTo(SubmissionStatus.Declined));
            Assert.That(entity.Status, Is.EqualTo(SubmissionStatus.Declined));
        }
    }
}
