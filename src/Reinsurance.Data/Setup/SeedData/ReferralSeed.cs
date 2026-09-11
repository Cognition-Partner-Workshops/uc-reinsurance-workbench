using Reinsurance.Core.Domain.Referrals;

namespace Reinsurance.Data.Setup.SeedData
{
    public static class ReferralSeed
    {
        public static void Seed(ReinsuranceObjectContext context, SeedState state)
        {
            context.Set<ReferralRule>().AddRange(new[]
            {
                new ReferralRule { Code = "MAX-LAYER", Description = "Layer exceeds the maximum layer limit.", Active = true, Kind = ReferralRuleKind.MaxLayerLimit, Threshold = 150000000m },
                new ReferralRule { Code = "MIN-ROL", Description = "Rate on line is below the minimum.", Active = true, Kind = ReferralRuleKind.MinRateOnLine, Threshold = 0.02m },
                new ReferralRule { Code = "MAX-ROL", Description = "Rate on line is above the maximum.", Active = true, Kind = ReferralRuleKind.MaxRateOnLine, Threshold = 0.35m },
                new ReferralRule { Code = "RATING", Description = "Cedent rating is below the required level.", Active = true, Kind = ReferralRuleKind.CedentRatingBelow, TextValue = "B++" },
                new ReferralRule { Code = "AUTHORITY", Description = "Layer exceeds underwriter authority.", Active = true, Kind = ReferralRuleKind.UnderwriterAuthority },
                new ReferralRule { Code = "PORTFOLIO", Description = "Portfolio aggregate limit is breached.", Active = true, Kind = ReferralRuleKind.PortfolioLimitBreach }
            });
            context.Set<PortfolioLimit>().AddRange(new[]
            {
                new PortfolioLimit { RegionId = state.Regions[0].Id, MaxAggregateLimit = 300000000m, MaxPML250 = 300000000m, Description = "Southeast aggregate property limit." },
                new PortfolioLimit { RegionId = state.Regions[1].Id, MaxAggregateLimit = 350000000m, MaxPML250 = 350000000m, Description = "Gulf aggregate property limit." },
                new PortfolioLimit { PerilId = state.Perils[0].Id, MaxAggregateLimit = 500000000m, MaxPML250 = 450000000m, Description = "Earthquake portfolio limit." },
                new PortfolioLimit { PerilId = state.Perils[1].Id, MaxAggregateLimit = 450000000m, MaxPML250 = 400000000m, Description = "Windstorm portfolio limit." },
                new PortfolioLimit { RegionId = state.Regions[2].Id, PerilId = state.Perils[3].Id, MaxAggregateLimit = 225000000m, MaxPML250 = 200000000m, Description = "California wildfire limit." }
            });
            context.SaveChanges();
        }
    }
}
