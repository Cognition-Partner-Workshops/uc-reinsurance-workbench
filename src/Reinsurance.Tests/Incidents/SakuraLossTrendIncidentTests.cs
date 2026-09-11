using System;
using System.Collections.Generic;
using NUnit.Framework;
using Reinsurance.Core.Domain.Exposure;
using Reinsurance.Services.LossHistory;
using Reinsurance.Tests.Fakes;

namespace Reinsurance.Tests.Incidents
{
    /// <summary>
    /// Pins the Sakura General loss-history trend lookup failure.
    /// </summary>
    [TestFixture]
    [Category("PlantedIncident")]
    public class SakuraLossTrendIncidentTests
    {
        [Test]
        public void SakuraLossHistoryTrendLookupThrows()
        {
            var service = new LossHistoryService(new FakeRepository<LossEvent>(new[]
            {
                new LossEvent
                {
                    CedentId = 6,
                    LossDate = new DateTime(2026, 3, 2),
                    GroundUpLoss = 4250000m
                }
            }));

            Assert.Throws<KeyNotFoundException>(() => service.BurningCost(6, 5));
        }

        [Test]
        [Category("Unit")]
        public void HistoricalLossWithKnownTrendReturnsPositiveBurningCost()
        {
            var service = new LossHistoryService(new FakeRepository<LossEvent>(new[]
            {
                new LossEvent
                {
                    CedentId = 1,
                    LossDate = new DateTime(2021, 9, 15),
                    GroundUpLoss = 1750000m
                }
            }));

            Assert.That(service.BurningCost(1, 5), Is.GreaterThan(0m));
        }
    }
}
