using System.Collections.Generic;
using Reinsurance.Services.Exposure.Models;

namespace Reinsurance.Services.Exposure
{
    public partial interface IExposureService
    {
        ExposureSummary GetSummary(int submissionId);
        IDictionary<int, ExposureSummary> GetSummaries(IEnumerable<int> submissionIds);
        IList<ExposureRecordModel> GetRecords(int submissionId);
    }
}
