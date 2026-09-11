using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Domain.Treaties;

namespace Reinsurance.Data.Setup.SeedData
{
    public static class TreatySeed
    {
        public static void Seed(ReinsuranceObjectContext context, SeedState state)
        {
            for (var i = 0; i < state.Submissions.Count; i++)
            {
                var submission = state.Submissions[i];
                var treaty = new Treaty
                {
                    SubmissionId = submission.Id,
                    Name = i == 2 ? "Atlas Specialty Property Cat XoL 2026" : "Meridian Mutual Property Cat XoL 2026 " + (i + 1),
                    Type = i == 9 ? TreatyType.QuotaShare : TreatyType.PropertyCatXoL,
                    Currency = "USD",
                    Status = submission.Status == SubmissionStatus.Bound ? TreatyStatus.Bound : TreatyStatus.Quoted,
                    InceptionDate = submission.InceptionDate,
                    ExpiryDate = submission.ExpiryDate
                };
                treaty.Layers.Add(new TreatyLayer { LayerNumber = 1, Limit = 25000000m, Attachment = 25000000m, Reinstatements = 1, ReinstatementPremiumPct = 0.5m, SharePct = 1m, Currency = "USD" });
                treaty.Layers.Add(new TreatyLayer { LayerNumber = 2, Limit = 50000000m, Attachment = 50000000m, Reinstatements = 1, ReinstatementPremiumPct = 0.5m, SharePct = 1m, Currency = "USD" });
                if (i % 3 == 0 || i == 2)
                    treaty.Layers.Add(new TreatyLayer { LayerNumber = 3, Limit = i == 2 ? 25000000m : 100000000m, Attachment = i == 2 ? 300000000m : 100000000m, Reinstatements = 1, ReinstatementPremiumPct = 0.5m, SharePct = 1m, Currency = "USD" });
                state.Treaties.Add(treaty);
            }
            context.Set<Treaty>().AddRange(state.Treaties);
            context.SaveChanges();
        }
    }
}
