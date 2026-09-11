using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Reinsurance.Core.Domain.Exposure;
using Reinsurance.Core.Domain.Reference;
using Reinsurance.Services.Exposure;

namespace Reinsurance.Tests.Exposure
{
    [TestFixture]
    public class ExposureCalculatorTests
    {
        [Test]
        [Category("Unit")]
        public void SummarizeCalculatesBreakdownsAndRatio()
        {
            var region = new Region { Code = "US-SE", Name = "Southeastern United States" };
            var peril = new Peril { Code = "EQ", Name = "Earthquake" };
            var result = ExposureCalculator.Summarize(new List<ExposureRecord>
            {
                new ExposureRecord { Region = region, Peril = peril, TotalInsuredValue = 100m, RiskCount = 2 }
            }, 25m);

            Assert.That(result.TotalTiv, Is.EqualTo(100m));
            Assert.That(result.RiskCount, Is.EqualTo(2));
            Assert.That(result.PmlToTivRatio, Is.EqualTo(0.25m));
            Assert.That(result.ByRegion, Has.Count.EqualTo(1));
            Assert.That(result.ByPeril, Has.Count.EqualTo(1));
        }

        [Test]
        [Category("Unit")]
        public void SummarizeCalculatesSharesAndConcentration()
        {
            var region1 = new Region { Code = "US-SE", Name = "Southeast" };
            var region2 = new Region { Code = "US-GULF", Name = "Gulf" };
            var peril1 = new Peril { Code = "EQ", Name = "Earthquake" };
            var peril2 = new Peril { Code = "WS", Name = "Windstorm" };
            var result = ExposureCalculator.Summarize(new List<ExposureRecord>
            {
                new ExposureRecord { Region = region1, Peril = peril1, TotalInsuredValue = 75m, RiskCount = 1 },
                new ExposureRecord { Region = region2, Peril = peril1, TotalInsuredValue = 25m, RiskCount = 2 },
                new ExposureRecord { Region = region1, Peril = peril2, TotalInsuredValue = 50m, RiskCount = 3 }
            }, 30m);

            Assert.That(result.ByRegion.Sum(x => x.Tiv), Is.EqualTo(result.TotalTiv));
            Assert.That(result.ByPeril.Sum(x => x.Tiv), Is.EqualTo(result.TotalTiv));
            Assert.That(result.ByRegion.Sum(x => x.SharePct), Is.EqualTo(1m).Within(0.000001m));
            Assert.That(result.ByPeril.Sum(x => x.SharePct), Is.EqualTo(1m).Within(0.000001m));
            Assert.That(result.Concentration, Is.EqualTo(0.833333333333333333m).Within(0.000001m));
            Assert.That(result.PmlToTivRatio, Is.EqualTo(0.2m));
        }

        [Test]
        [Category("Unit")]
        public void EmptyRecordsReturnZeros()
        {
            var result = ExposureCalculator.Summarize(null, 100m);

            Assert.That(result.TotalTiv, Is.EqualTo(0m));
            Assert.That(result.RiskCount, Is.EqualTo(0));
            Assert.That(result.ByRegion, Is.Empty);
            Assert.That(result.ByPeril, Is.Empty);
            Assert.That(result.Concentration, Is.EqualTo(0m));
            Assert.That(result.PmlToTivRatio, Is.EqualTo(0m));
        }
    }
}
