using System;
using NUnit.Framework;
using Reinsurance.Core.Domain.Exposure;
using Reinsurance.Services.LossHistory;
using Reinsurance.Tests.Fakes;

namespace Reinsurance.Tests.Incidents
{
    /// <summary>
    /// Regression coverage for the Sakura General loss whose year is missing from the trend-factor table (Sentry BACKEND-REINSURANCE-DEMO-4).
    /// </summary>
    [TestFixture]
    [Category("PlantedIncident")]
    public class SakuraLossTrendIncidentTests
    {
        private static readonly LossEvent HyugaNadaEarthquake2026 = new LossEvent
        {
            CedentId = 6,
            LossDate = new DateTime(2026, 3, 2),
            GroundUpLoss = 4250000m
        };

        [Test]
        public void SakuraLossHistoryWithUntabulatedYearDoesNotThrow()
        {
            var service = new LossHistoryService(new FakeRepository<LossEvent>(new[] { HyugaNadaEarthquake2026 }));

            decimal burningCost = 0m;
            Assert.DoesNotThrow(() => burningCost = service.BurningCost(6, 5));
            Assert.That(burningCost, Is.EqualTo(4250000m / 5));
        }

        [Test]
        public void SakuraLossHistoryIncludesRecentLossAtCurrentCostLevel()
        {
            var service = new LossHistoryService(new FakeRepository<LossEvent>(new[]
            {
                HyugaNadaEarthquake2026,
                new LossEvent
                {
                    CedentId = 6,
                    LossDate = new DateTime(2025, 9, 15),
                    GroundUpLoss = 1000000m
                }
            }));

            Assert.That(service.BurningCost(6, 5), Is.EqualTo((4250000m + 1000000m) / 5));
        }

        [Test]
        [Category("Unit")]
        public void TrendFactorClampsYearsOutsideTheTable()
        {
            Assert.That(LossHistoryService.TrendFactor(2026), Is.EqualTo(LossHistoryService.TrendFactor(2025)));
            Assert.That(LossHistoryService.TrendFactor(2030), Is.EqualTo(LossHistoryService.TrendFactor(2025)));
            Assert.That(LossHistoryService.TrendFactor(2015), Is.EqualTo(LossHistoryService.TrendFactor(2018)));
            Assert.That(LossHistoryService.TrendFactor(2021), Is.EqualTo(1.21m));
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
