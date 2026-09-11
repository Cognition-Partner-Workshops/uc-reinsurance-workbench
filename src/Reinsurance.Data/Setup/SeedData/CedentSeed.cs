using System.Collections.Generic;
using Reinsurance.Core.Domain.Cedents;

namespace Reinsurance.Data.Setup.SeedData
{
    public static class CedentSeed
    {
        public static void Seed(ReinsuranceObjectContext context, SeedState state)
        {
            state.Cedents = new List<Cedent>
            {
                new Cedent { Name = "Meridian Mutual", Code = "CED-001", Country = "US", Rating = "A+", Active = true },
                new Cedent { Name = "Gulfstream P&C", Code = "CED-002", Country = "US", Rating = "A", Active = true },
                new Cedent { Name = "Pacific Crest Insurance", Code = "CED-003", Country = "US", Rating = "A", Active = true },
                new Cedent { Name = "Northwind Assurance", Code = "CED-004", Country = "CA", Rating = "A-", Active = true },
                new Cedent { Name = "Iberia Seguros", Code = "CED-005", Country = "ES", Rating = "B++", Active = true },
                new Cedent { Name = "Sakura General", Code = "CED-006", Country = "JP", Rating = "B+", Active = true }
            };
            state.Brokers = new List<Broker>
            {
                new Broker { Name = "Halcyon Re Brokers", Code = "BRK-001" },
                new Broker { Name = "Crestline Intermediaries", Code = "BRK-002" },
                new Broker { Name = "Northgate Re", Code = "BRK-003" }
            };
            state.Underwriters = new List<Underwriter>
            {
                new Underwriter { Name = "Priya Raman", Email = "priya.raman@example.invalid", AuthorityLimit = 25000000m, Active = true },
                new Underwriter { Name = "Tomas Keller", Email = "tomas.keller@example.invalid", AuthorityLimit = 50000000m, Active = true },
                new Underwriter { Name = "Aisha Bello", Email = "aisha.bello@example.invalid", AuthorityLimit = 100000000m, Active = true },
                new Underwriter { Name = "Daniel Okafor", Email = "daniel.okafor@example.invalid", AuthorityLimit = 250000000m, Active = true }
            };
            context.Set<Cedent>().AddRange(state.Cedents);
            context.Set<Broker>().AddRange(state.Brokers);
            context.Set<Underwriter>().AddRange(state.Underwriters);
            context.SaveChanges();
        }
    }
}
