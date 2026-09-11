using System.Collections.Generic;

namespace Reinsurance.Services.Exposure.Models
{
    public sealed class ExposureSummary
    {
        public decimal TotalTiv { get; set; }
        public IList<ExposureBreakdown> ByRegion { get; set; } = new List<ExposureBreakdown>();
        public IList<ExposureBreakdown> ByPeril { get; set; } = new List<ExposureBreakdown>();
        public decimal Concentration { get; set; }
        public decimal PmlToTivRatio { get; set; }
        public int RiskCount { get; set; }
    }
}
