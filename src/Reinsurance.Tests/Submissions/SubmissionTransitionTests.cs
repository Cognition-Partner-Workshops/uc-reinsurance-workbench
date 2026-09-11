using System;
using NUnit.Framework;
using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Exceptions;
using Reinsurance.Services.Submissions;
using Reinsurance.Services.Submissions.Models;
using Reinsurance.Tests.Fakes;

namespace Reinsurance.Tests.Submissions
{
    [TestFixture]
    public class SubmissionTransitionTests
    {
        [Test]
        [Category("Unit")]
        public void ReceivedToInReviewSucceeds()
        {
            var service = Service(new Submission { Id = 1, Status = SubmissionStatus.Received });

            Assert.That(service.Transition(1, SubmissionStatus.InReview).Status, Is.EqualTo(SubmissionStatus.InReview));
        }

        [Test]
        [Category("Unit")]
        public void ReceivedToBoundThrows()
        {
            var service = Service(new Submission { Id = 1, Status = SubmissionStatus.Received });

            Assert.Throws<InvalidSubmissionTransitionException>(() => service.Transition(1, SubmissionStatus.Bound));
        }

        [Test]
        [Category("Unit")]
        public void BoundToWithdrawnThrows()
        {
            var service = Service(new Submission { Id = 1, Status = SubmissionStatus.Bound });

            Assert.Throws<InvalidSubmissionTransitionException>(() => service.Transition(1, SubmissionStatus.Withdrawn));
        }

        [Test]
        [Category("Unit")]
        public void InReviewToWithdrawnSucceeds()
        {
            var service = Service(new Submission { Id = 1, Status = SubmissionStatus.InReview });

            Assert.That(service.Transition(1, SubmissionStatus.Withdrawn).Status, Is.EqualTo(SubmissionStatus.Withdrawn));
        }

        [Test]
        [Category("Unit")]
        public void DeclinedCannotTransition()
        {
            var service = Service(new Submission { Id = 1, Status = SubmissionStatus.Declined });

            Assert.Throws<InvalidSubmissionTransitionException>(() => service.Transition(1, SubmissionStatus.Received));
        }

        [Test]
        [Category("Unit")]
        public void CreateGeneratesYearlyReferences()
        {
            var repository = new FakeRepository<Submission>();
            var service = new SubmissionService(repository, new EmptyExposureService(), new FakeRepository<PricingResult>());

            var first = service.Create(new SubmissionCreateRequest
            {
                CedentId = 1,
                InceptionDate = new DateTime(2026, 1, 1),
                ExpiryDate = new DateTime(2026, 12, 31)
            });
            var second = service.Create(new SubmissionCreateRequest
            {
                CedentId = 1,
                InceptionDate = new DateTime(2026, 2, 1),
                ExpiryDate = new DateTime(2026, 12, 31)
            });

            Assert.That(first.Reference, Is.EqualTo("SUB-2026-0001"));
            Assert.That(second.Reference, Is.EqualTo("SUB-2026-0002"));
        }

        private static SubmissionService Service(Submission submission)
        {
            return new SubmissionService(
                new FakeRepository<Submission>(new[] { submission }),
                new EmptyExposureService(),
                new FakeRepository<PricingResult>());
        }
    }
}
