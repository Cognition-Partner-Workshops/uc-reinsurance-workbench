using System.Collections.Generic;
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
    }
}
