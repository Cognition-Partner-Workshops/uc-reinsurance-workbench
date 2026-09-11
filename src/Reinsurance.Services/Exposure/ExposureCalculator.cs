using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Domain.Exposure;
using Reinsurance.Services.Exposure.Models;

namespace Reinsurance.Services.Exposure
{
    public static class ExposureCalculator
    {
        public static ExposureSummary Summarize(IEnumerable<ExposureRecord> records, decimal pml250)
        {
            var list = (records ?? Enumerable.Empty<ExposureRecord>()).ToList();
            var total = list.Sum(x => x.TotalInsuredValue);
            var byRegion = list.GroupBy(x => x.Region).Select(x => new ExposureBreakdown
            {
                RegionCode = x.Key == null ? "UNKNOWN" : x.Key.Code,
                RegionName = x.Key == null ? "Unknown" : x.Key.Name,
                Tiv = x.Sum(y => y.TotalInsuredValue),
                SharePct = total == 0m ? 0m : x.Sum(y => y.TotalInsuredValue) / total
            }).OrderByDescending(x => x.Tiv).ToList();
            var byPeril = list.GroupBy(x => x.Peril).Select(x => new ExposureBreakdown
            {
                PerilCode = x.Key == null ? "UNKNOWN" : x.Key.Code,
                PerilName = x.Key == null ? "Unknown" : x.Key.Name,
                Tiv = x.Sum(y => y.TotalInsuredValue),
                SharePct = total == 0m ? 0m : x.Sum(y => y.TotalInsuredValue) / total
            }).OrderByDescending(x => x.Tiv).ToList();
            return new ExposureSummary
            {
                TotalTiv = total,
                RiskCount = list.Sum(x => x.RiskCount),
                ByRegion = byRegion,
                ByPeril = byPeril,
                Concentration = byRegion.Count == 0 ? 0m : byRegion[0].SharePct,
                PmlToTivRatio = total == 0m ? 0m : pml250 / total
            };
        }
    }
}
