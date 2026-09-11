using System;
using System.Linq;
using NUnit.Framework;
using Reinsurance.Core.Domain.CatModel;
using Reinsurance.Core.Domain.Cedents;
using Reinsurance.Core.Domain.Exposure;
using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Reference;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Domain.Treaties;
using Reinsurance.Services.CatModel;
using Reinsurance.Services.Exposure;
using Reinsurance.Services.Portfolio;
using Reinsurance.Services.Pricing;
using Reinsurance.Services.Referrals;
using Reinsurance.Services.Submissions;
using Reinsurance.Tests.Fakes;

namespace Reinsurance.Tests.Portfolio
{
    [TestFixture]
    public class PortfolioServiceTests
    {
        private sealed class NoReferrals : IReferralService
        {
            public ReferralDecision Evaluate(TreatyLayer layer, PricingResult pricing, Submission submission)
            {
                return new ReferralDecision();
            }
        }

        private static PortfolioService Service()
        {
            var cedent = new Cedent { Id = 1, Name = "Meridian Mutual", Rating = "A" };
            var windstorm = new Peril { Id = 1, Code = "WS", Name = "Windstorm" };
            var region = new Region { Id = 1, Code = "US-SE", Name = "US South East" };

            var layer1 = new TreatyLayer { Id = 100, TreatyId = 10, LayerNumber = 1, Limit = 50000000m, Attachment = 50000000m, SharePct = 1m, Currency = "USD" };
            var layer2 = new TreatyLayer { Id = 200, TreatyId = 20, LayerNumber = 1, Limit = 25000000m, Attachment = 25000000m, SharePct = 0.5m, Currency = "USD" };
            var treaty1 = new Treaty { Id = 10, SubmissionId = 1, Name = "Meridian Cat XoL", Status = TreatyStatus.Bound, Currency = "USD", Layers = { layer1 } };
            var treaty2 = new Treaty { Id = 20, SubmissionId = 2, Name = "Gulfstream Cat XoL", Status = TreatyStatus.Quoted, Currency = "USD", Layers = { layer2 } };
            layer1.Treaty = treaty1;
            layer2.Treaty = treaty2;

            var submissions = new[]
            {
                new Submission { Id = 1, Reference = "SUB-2026-0001", CedentId = 1, Cedent = cedent, Status = SubmissionStatus.Bound, Treaties = { treaty1 } },
                new Submission { Id = 2, Reference = "SUB-2026-0002", CedentId = 1, Cedent = cedent, Status = SubmissionStatus.Quoted, Treaties = { treaty2 } },
                new Submission { Id = 3, Reference = "SUB-2026-0003", CedentId = 1, Cedent = cedent, Status = SubmissionStatus.Withdrawn, Deleted = true }
            };
            var pricing = new FakeRepository<PricingResult>(new[]
            {
                new PricingResult { Id = 1, TreatyLayerId = 100, TreatyLayer = layer1, TechnicalPremium = 4000000m, CalculatedOn = DateTime.UtcNow },
                new PricingResult { Id = 2, TreatyLayerId = 200, TreatyLayer = layer2, TechnicalPremium = 1500000m, CalculatedOn = DateTime.UtcNow }
            });
            var catModels = new FakeRepository<CatModelResult>(new[]
            {
                new CatModelResult { Id = 1, SubmissionId = 1, PML250 = 120000000m, RunOn = new DateTime(2026, 1, 1) },
                new CatModelResult { Id = 2, SubmissionId = 1, PML250 = 130000000m, RunOn = new DateTime(2026, 2, 1) },
                new CatModelResult { Id = 3, SubmissionId = 2, PML250 = 80000000m, RunOn = new DateTime(2026, 2, 1) },
                new CatModelResult { Id = 4, SubmissionId = 3, PML250 = 999000000m, RunOn = new DateTime(2026, 2, 1) }
            });
            var exposures = new FakeRepository<ExposureRecord>(new[]
            {
                new ExposureRecord { Id = 1, SubmissionId = 1, Region = region, Peril = windstorm, TotalInsuredValue = 900000000m, RiskCount = 10 },
                new ExposureRecord { Id = 2, SubmissionId = 2, Region = region, Peril = windstorm, TotalInsuredValue = 400000000m, RiskCount = 5 }
            });

            var exposureService = new ExposureService(exposures, catModels);
            var catModelService = new CatModelResultService(catModels);
            return new PortfolioService(
                new SubmissionService(new FakeRepository<Submission>(submissions), exposureService, pricing),
                new PricingService(new FakeRepository<TreatyLayer>(new[] { layer1, layer2 }), pricing, new FakeRepository<Treaty>(new[] { treaty1, treaty2 }), catModelService, new NoReferrals()),
                catModelService,
                exposureService);
        }

        [Test]
        [Category("Unit")]
        public void ReturnsEveryLiveSubmissionWithTreatiesAndLayers()
        {
            var portfolio = Service().Get();

            Assert.That(portfolio.Submissions.Select(x => x.Id), Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(portfolio.Submissions.SelectMany(x => x.Treaties).Select(x => x.Id), Is.EquivalentTo(new[] { 10, 20 }));
            Assert.That(portfolio.Submissions.SelectMany(x => x.Treaties).SelectMany(x => x.Layers).Count(), Is.EqualTo(2));
        }

        [Test]
        [Category("Unit")]
        public void ReturnsPricingKeyedByTreaty()
        {
            var portfolio = Service().Get();

            Assert.That(portfolio.TreatyPricing.Select(x => x.TreatyId), Is.EquivalentTo(new[] { 10, 20 }));
            Assert.That(portfolio.TreatyPricing.Single(x => x.TreatyId == 10).Results.Single().TechnicalPremium, Is.EqualTo(4000000m));
            Assert.That(portfolio.TreatyPricing.Single(x => x.TreatyId == 20).Results.Single().OurShareLine, Is.EqualTo(750000m));
        }

        [Test]
        [Category("Unit")]
        public void ReturnsCatModelsAndExposureKeyedBySubmission()
        {
            var portfolio = Service().Get();

            Assert.That(portfolio.CatModels.Select(x => x.SubmissionId), Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(portfolio.CatModels.Single(x => x.SubmissionId == 1).Results.Select(x => x.PML250), Is.EqualTo(new[] { 130000000m, 120000000m }));
            Assert.That(portfolio.Exposures.Select(x => x.SubmissionId), Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(portfolio.Exposures.Single(x => x.SubmissionId == 1).Summary.TotalTiv, Is.EqualTo(900000000m));
            Assert.That(portfolio.Exposures.Single(x => x.SubmissionId == 2).Summary.ByPeril.Single().PerilName, Is.EqualTo("Windstorm"));
        }

        [Test]
        [Category("Unit")]
        public void EmptyPortfolioReturnsEmptyCollections()
        {
            var pricing = new FakeRepository<PricingResult>();
            var catModels = new FakeRepository<CatModelResult>();
            var exposureService = new ExposureService(new FakeRepository<ExposureRecord>(), catModels);
            var catModelService = new CatModelResultService(catModels);
            var service = new PortfolioService(
                new SubmissionService(new FakeRepository<Submission>(), exposureService, pricing),
                new PricingService(new FakeRepository<TreatyLayer>(), pricing, new FakeRepository<Treaty>(), catModelService, new NoReferrals()),
                catModelService,
                exposureService);

            var portfolio = service.Get();

            Assert.That(portfolio.Submissions, Is.Empty);
            Assert.That(portfolio.TreatyPricing, Is.Empty);
            Assert.That(portfolio.CatModels, Is.Empty);
            Assert.That(portfolio.Exposures, Is.Empty);
        }
    }
}
