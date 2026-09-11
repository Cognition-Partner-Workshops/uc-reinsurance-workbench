using System;
using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Domain.CatModel;
using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Submissions;

namespace Reinsurance.Data.Setup.SeedData
{
    public static class CatModelSeed
    {
        public static void Seed(ReinsuranceObjectContext context, SeedState state)
        {
            var models = new List<CatModelResult>();
            var vendors = new[] { "RMS", "AIR", "Internal" };
            var versions = new[] { "v23", "v10", "v2026.1" };
            for (var i = 0; i < state.Submissions.Count; i++)
            {
                var pml250 = i == 2 ? 300000000m : 250000000m + i * 15000000m;
                var model = new CatModelResult
                {
                    SubmissionId = state.Submissions[i].Id,
                    ModelVendor = vendors[i % vendors.Length],
                    ModelVersion = versions[i % versions.Length],
                    AAL = pml250 * 0.03m,
                    ExpectedLoss = pml250 * 0.025m,
                    PML50 = pml250 * 0.35m,
                    PML100 = pml250 * 0.65m,
                    PML250 = pml250,
                    RunOn = new DateTime(2026, 2, 15).AddDays(i)
                };
                models.Add(model);
                state.CatModels.Add(model);
                for (var j = 0; j < state.Perils.Count; j++)
                {
                    models.Add(new CatModelResult
                    {
                        SubmissionId = state.Submissions[i].Id,
                        PerilId = state.Perils[j].Id,
                        ModelVendor = vendors[(i + j) % vendors.Length],
                        ModelVersion = versions[(i + j) % versions.Length],
                        AAL = pml250 * 0.006m,
                        ExpectedLoss = pml250 * 0.005m,
                        PML50 = pml250 * 0.10m,
                        PML100 = pml250 * 0.20m,
                        PML250 = pml250 * 0.30m,
                        RunOn = model.RunOn
                    });
                }
            }
            context.Set<CatModelResult>().AddRange(models);
            context.SaveChanges();
            var prices = new List<PricingResult>();
            foreach (var treaty in state.Treaties)
            {
                if (state.Submissions.First(x => x.Id == treaty.SubmissionId).Status != SubmissionStatus.Bound)
                    continue;
                foreach (var layer in treaty.Layers)
                {
                    prices.Add(new PricingResult
                    {
                        TreatyLayerId = layer.Id,
                        TechnicalPremium = layer.Limit * 0.05m,
                        ExpectedLoss = layer.Limit * 0.02m,
                        ExpenseLoad = layer.Limit * 0.005m,
                        RiskLoad = layer.Limit * 0.01m,
                        RateOnLine = 0.05m,
                        LossCostPct = 0.02m,
                        ProfitMarginPct = 0.08m,
                        ReferralRequired = false,
                        ReferralReasons = string.Empty,
                        CalculatedOn = new DateTime(2026, 2, 20),
                        CreatedOnUtc = new DateTime(2026, 2, 20),
                        UpdatedOnUtc = new DateTime(2026, 2, 20)
                    });
                }
            }
            context.Set<PricingResult>().AddRange(prices);
            context.SaveChanges();
        }
    }
}
