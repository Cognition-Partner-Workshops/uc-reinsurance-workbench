using System.Collections.Generic;
using System.Linq;
using Reinsurance.Services.Exposure;
using Reinsurance.Services.Exposure.Models;

namespace Reinsurance.Tests.Fakes
{
    public sealed class EmptyExposureService : IExposureService
    {
        public ExposureSummary GetSummary(int submissionId)
        {
            return new ExposureSummary();
        }

        public IDictionary<int, ExposureSummary> GetSummaries(IEnumerable<int> submissionIds)
        {
            return submissionIds.Distinct().ToDictionary(id => id, id => new ExposureSummary());
        }

        public IList<ExposureRecordModel> GetRecords(int submissionId)
        {
            return new List<ExposureRecordModel>();
        }
    }
}
