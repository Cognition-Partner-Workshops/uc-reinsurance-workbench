using System;
using System.Collections.Generic;
using Reinsurance.Core.Domain.Exposure;

namespace Reinsurance.Data.Setup.SeedData
{
    public static class ExposureSeed
    {
        public static void Seed(ReinsuranceObjectContext context, SeedState state)
        {
            var random = new Random(20260911);
            var records = new List<ExposureRecord>();
            for (var i = 0; i < state.Submissions.Count; i++)
            {
                for (var j = 0; j < 10; j++)
                {
                    records.Add(new ExposureRecord
                    {
                        SubmissionId = state.Submissions[i].Id,
                        RegionId = state.Regions[(i + j) % state.Regions.Count].Id,
                        PerilId = state.Perils[(i + j) % state.Perils.Count].Id,
                        TotalInsuredValue = 25000000m + random.Next(0, 10000000),
                        RiskCount = 80 + random.Next(120),
                        AverageDeductiblePct = 0.02m + j * 0.001m
                    });
                }
            }
            context.Set<ExposureRecord>().AddRange(records);
            context.SaveChanges();
        }
    }
}
