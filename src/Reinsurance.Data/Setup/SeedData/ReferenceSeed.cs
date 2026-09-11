using System.Collections.Generic;
using Reinsurance.Core.Domain.Reference;

namespace Reinsurance.Data.Setup.SeedData
{
    public static class ReferenceSeed
    {
        public static void Seed(ReinsuranceObjectContext context, SeedState state)
        {
            state.Perils = new List<Peril>
            {
                new Peril { Code = "EQ", Name = "Earthquake" },
                new Peril { Code = "WS", Name = "Windstorm" },
                new Peril { Code = "FL", Name = "Flood" },
                new Peril { Code = "WF", Name = "Wildfire" },
                new Peril { Code = "SCS", Name = "Severe Convective Storm" }
            };
            state.Regions = new List<Region>
            {
                new Region { Code = "US-SE", Name = "United States Southeast", CountryCode = "US" },
                new Region { Code = "US-GULF", Name = "United States Gulf Coast", CountryCode = "US" },
                new Region { Code = "US-CA", Name = "California", CountryCode = "US" },
                new Region { Code = "US-NE", Name = "United States Northeast", CountryCode = "US" },
                new Region { Code = "EU-W", Name = "Western Europe", CountryCode = "EU" },
                new Region { Code = "JP", Name = "Japan", CountryCode = "JP" },
                new Region { Code = "AUS", Name = "Australia", CountryCode = "AU" },
                new Region { Code = "CARIB", Name = "Caribbean", CountryCode = "CAR" }
            };
            context.Set<Peril>().AddRange(state.Perils);
            context.Set<Region>().AddRange(state.Regions);
            context.SaveChanges();
        }
    }
}
