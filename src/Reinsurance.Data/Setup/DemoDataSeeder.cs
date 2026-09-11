using System.Linq;
using Reinsurance.Data.Setup.SeedData;

namespace Reinsurance.Data.Setup
{
    public static class DemoDataSeeder
    {
        public static void Seed(ReinsuranceObjectContext context)
        {
            if (context.Set<Reinsurance.Core.Domain.Cedents.Cedent>().Any())
                return;
            var state = new SeedState();
            ReferenceSeed.Seed(context, state);
            CedentSeed.Seed(context, state);
            SubmissionSeed.Seed(context, state);
            TreatySeed.Seed(context, state);
            ExposureSeed.Seed(context, state);
            CatModelSeed.Seed(context, state);
            LossHistorySeed.Seed(context, state);
            ReferralSeed.Seed(context, state);
        }
    }
}
