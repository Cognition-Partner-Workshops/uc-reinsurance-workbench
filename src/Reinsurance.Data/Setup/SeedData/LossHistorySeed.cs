using System;
using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Domain.Exposure;

namespace Reinsurance.Data.Setup.SeedData
{
    public static class LossHistorySeed
    {
        public static void Seed(ReinsuranceObjectContext context, SeedState state)
        {
            var events = new[] { "Hurricane Ian 2022", "Winter Storm Uri 2021", "Tohoku EQ 2011", "Kaikoura EQ 2016", "Camp Fire 2018", "Bernd Floods 2021", "Hurricane Ida 2021" };
            var losses = new List<LossEvent>();
            for (var i = 0; i < state.Cedents.Count; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    losses.Add(new LossEvent
                    {
                        CedentId = state.Cedents[i].Id,
                        RegionId = state.Regions[(i + j) % state.Regions.Count].Id,
                        PerilId = state.Perils[(i + j) % state.Perils.Count].Id,
                        EventName = events[(i + j) % events.Length],
                        LossDate = new DateTime(2018 + j, 9, 15),
                        GroundUpLoss = (i + j + 1) * 1750000m,
                        CededLoss = (i + j + 1) * 300000m
                    });
                }
            }
            var japanCedent = state.Cedents.Single(x => x.Country == "JP");
            var japanRegion = state.Regions.Single(x => x.Code == "JP");
            var earthquake = state.Perils.Single(x => x.Code == "EQ");
            losses.Add(new LossEvent
            {
                CedentId = japanCedent.Id,
                RegionId = japanRegion.Id,
                PerilId = earthquake.Id,
                EventName = "Hyuga-nada EQ 2026",
                LossDate = new DateTime(2026, 3, 2),
                GroundUpLoss = 4250000m,
                CededLoss = 900000m
            });
            context.Set<LossEvent>().AddRange(losses);
            context.SaveChanges();
        }
    }
}
