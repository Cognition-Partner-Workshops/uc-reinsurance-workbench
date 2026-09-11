using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Domain;

namespace Reinsurance.Services.Exposure
{
    public sealed class ExposureSummary
    {
        public decimal TotalInsuredValue { get; set; }
        public int RiskCount { get; set; }
        public int RecordCount { get; set; }
        public IDictionary<string, decimal> ByPeril { get; set; }
    }

    public static class ExposureCalculator
    {
        public static ExposureSummary Summarize(IEnumerable<ExposureRecord> records)
        {
            var list = (records ?? Enumerable.Empty<ExposureRecord>()).ToList();
            return new ExposureSummary
            {
                TotalInsuredValue = list.Sum(x => x.TotalInsuredValue),
                RiskCount = list.Sum(x => x.RiskCount),
                RecordCount = list.Count,
                ByPeril = list.GroupBy(x => x.Peril == null ? "Unknown" : x.Peril.Code).ToDictionary(x => x.Key, x => x.Sum(y => y.TotalInsuredValue))
            };
        }
    }
}
