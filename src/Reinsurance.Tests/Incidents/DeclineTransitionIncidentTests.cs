using System;
using NUnit.Framework;
using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Services.Exposure;
using Reinsurance.Services.Submissions;
using Reinsurance.Tests.Fakes;

namespace Reinsurance.Tests.Incidents
{
    /// <summary>
    /// Pins the quoted-submission decline transition failure.
    /// </summary>
    [TestFixture]
    [Category("PlantedIncident")]
    public class DeclineTransitionIncidentTests
    {
        [Test]
        public void DecliningQuotedSubmissionWithoutPricingThrowsBeforeMutation()
        {
            var entity = new Submission { Id = 1, Status = SubmissionStatus.Quoted };
            var service = new SubmissionService(
                new FakeRepository<Submission>(new[] { entity }),
                new EmptyExposureService(),
                new FakeRepository<PricingResult>());

            Assert.Throws<InvalidOperationException>(
                () => service.Transition(1, SubmissionStatus.Declined));
            Assert.That(entity.Status, Is.EqualTo(SubmissionStatus.Quoted));
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
    }
}
