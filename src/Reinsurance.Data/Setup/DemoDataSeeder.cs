using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Reinsurance.Core.Domain;

namespace Reinsurance.Data.Setup
{
    public static class DemoDataSeeder
    {
        public static void Seed(ReinsuranceObjectContext context)
        {
            if (context.Set<Cedent>().Any()) return;
            var now = DateTime.UtcNow;
            var perils = new[] { "EQ", "WS", "FL", "WF", "ST" }.Select((code, i) => new Peril { Code = code, Name = new[] { "Earthquake", "Windstorm", "Flood", "Wildfire", "Severe Thunderstorm" }[i] }).ToList();
            var regions = new[] { "NA", "EU", "APAC", "LATAM", "MEA", "UK", "ANZ", "CAR" }.Select((code, i) => new Region { Code = code, Name = "Region " + code, CountryCode = code }).ToList();
            var cedents = new[] { "Blue Harbor Insurance", "Cedar Mutual", "Northstar Assurance", "Pacific Shield", "Summit Indemnity", "Westbridge Underwriting" }.Select((name, i) => new Cedent { Name = name, Code = "CED" + (i + 1), Country = "US", Rating = i < 2 ? "A" : "BBB", Active = true, CreatedOnUtc = now, UpdatedOnUtc = now }).ToList();
            var brokers = new[] { "Apex Re Brokers", "Harbor Street Re", "Summit Risk Partners" }.Select((name, i) => new Broker { Name = name, Code = "BRK" + (i + 1) }).ToList();
            var underwriters = new[] { 25000000m, 50000000m, 100000000m, 250000000m }.Select((limit, i) => new Underwriter { Name = "Underwriter " + (i + 1), Email = "uw" + (i + 1) + "@example.invalid", AuthorityLimit = limit, Active = true }).ToList();
            context.Set<Peril>().AddRange(perils);
            context.Set<Region>().AddRange(regions);
            context.Set<Cedent>().AddRange(cedents);
            context.Set<Broker>().AddRange(brokers);
            context.Set<Underwriter>().AddRange(underwriters);
            context.SaveChanges();

            var submissions = new List<Submission>();
            for (var i = 0; i < 10; i++)
            {
                submissions.Add(new Submission
                {
                    Reference = "SUB-2026-" + (i + 1).ToString("000"),
                    CedentId = cedents[i % cedents.Count].Id,
                    BrokerId = brokers[i % brokers.Count].Id,
                    UnderwriterId = underwriters[i % underwriters.Count].Id,
                    ReceivedOn = now.AddDays(-i),
                    InceptionDate = new DateTime(2026, 1, 1),
                    ExpiryDate = new DateTime(2026, 12, 31),
                    Status = i == 0 ? SubmissionStatus.InReview : SubmissionStatus.Received,
                    Notes = "Deterministic demonstration submission.",
                    CreatedOnUtc = now,
                    UpdatedOnUtc = now
                });
            }
            context.Set<Submission>().AddRange(submissions);
            context.SaveChanges();

            var models = new List<CatModelResult>();
            var exposures = new List<ExposureRecord>();
            var treaties = new List<Treaty>();
            var random = new Random(20260911);
            for (var i = 0; i < submissions.Count; i++)
            {
                var pml250 = i == 0 ? 300000000m : (i == 1 ? 100000000m : 220000000m + i * 10000000m);
                models.Add(new CatModelResult { SubmissionId = submissions[i].Id, ModelVendor = "DemoCat", ModelVersion = "2026.1", AAL = i == 0 ? 12000000m : 8500000m + i * 250000m, ExpectedLoss = 8000000m + i * 100000m, PML50 = pml250 * 0.35m, PML100 = pml250 * 0.65m, PML250 = pml250, RunOn = now });
                for (var j = 0; j < 8; j++)
                    exposures.Add(new ExposureRecord { SubmissionId = submissions[i].Id, RegionId = regions[(i + j) % regions.Count].Id, PerilId = perils[(i + j) % perils.Count].Id, TotalInsuredValue = 10000000m + random.Next(0, 5000000), RiskCount = 20 + random.Next(40), AverageDeductiblePct = 0.02m + (j * 0.001m) });
                treaties.Add(new Treaty { SubmissionId = submissions[i].Id, Name = i == 1 ? "Atlas Re Property Cat 2026" : "Treaty " + (i + 1), Type = TreatyType.PropertyCatXoL, Currency = "USD", Status = TreatyStatus.Quoted, InceptionDate = new DateTime(2026, 1, 1), ExpiryDate = new DateTime(2026, 12, 31), CreatedOnUtc = now, UpdatedOnUtc = now });
            }
            context.Set<CatModelResult>().AddRange(models);
            context.Set<ExposureRecord>().AddRange(exposures);
            context.Set<Treaty>().AddRange(treaties);
            context.SaveChanges();

            var layers = new List<TreatyLayer>();
            for (var i = 0; i < treaties.Count; i++)
            {
                var pml = models[i].PML250;
                layers.Add(new TreatyLayer { TreatyId = treaties[i].Id, LayerNumber = 1, Limit = 25000000m, Attachment = i == 1 ? 25000000m : 25000000m, Reinstatements = 1, ReinstatementPremiumPct = 0.5m, SharePct = 1m, Currency = "USD" });
                layers.Add(new TreatyLayer { TreatyId = treaties[i].Id, LayerNumber = 2, Limit = 50000000m, Attachment = 75000000m, Reinstatements = 1, ReinstatementPremiumPct = 0.5m, SharePct = 1m, Currency = "USD" });
                if (i == 1)
                    layers.Add(new TreatyLayer { TreatyId = treaties[i].Id, LayerNumber = 3, Limit = 25000000m, Attachment = pml, Reinstatements = 1, ReinstatementPremiumPct = 0.5m, SharePct = 1m, Currency = "USD" });
            }
            context.Set<TreatyLayer>().AddRange(layers);
            var rules = new[]
            {
                new ReferralRule { Code = "MAX-LIMIT", Description = "Layer limit exceeds referral threshold.", Active = true, Kind = ReferralRuleKind.MaxLayerLimit, Threshold = 100000000m },
                new ReferralRule { Code = "MIN-ROL", Description = "Rate on line is below minimum.", Active = true, Kind = ReferralRuleKind.MinRateOnLine, Threshold = 0.02m },
                new ReferralRule { Code = "MAX-ROL", Description = "Rate on line exceeds maximum.", Active = true, Kind = ReferralRuleKind.MaxRateOnLine, Threshold = 0.5m },
                new ReferralRule { Code = "RATING", Description = "Cedent rating requires review.", Active = true, Kind = ReferralRuleKind.CedentRatingBelow, TextValue = "BBB" },
                new ReferralRule { Code = "AUTHORITY", Description = "Layer exceeds underwriter authority.", Active = true, Kind = ReferralRuleKind.UnderwriterAuthority },
                new ReferralRule { Code = "PORTFOLIO", Description = "Portfolio aggregate limit requires review.", Active = true, Kind = ReferralRuleKind.PortfolioLimitBreach }
            };
            context.Set<ReferralRule>().AddRange(rules);
            context.Set<PortfolioLimit>().AddRange(regions.Take(5).Select((region, i) => new PortfolioLimit { RegionId = region.Id, MaxAggregateLimit = 500000000m + i * 25000000m, MaxPML250 = 350000000m, Description = "Regional aggregate limit." }));
            context.SaveChanges();

            var losses = new List<LossEvent>();
            foreach (var cedent in cedents)
                for (var i = 0; i < 4; i++)
                    losses.Add(new LossEvent { CedentId = cedent.Id, RegionId = regions[i % regions.Count].Id, PerilId = perils[i % perils.Count].Id, EventName = "Demo event " + (i + 1), LossDate = new DateTime(2023 + i, 6, 15), GroundUpLoss = 1000000m * (i + 1), CededLoss = 250000m * i });
            context.Set<LossEvent>().AddRange(losses);
            context.SaveChanges();
        }
    }
}
