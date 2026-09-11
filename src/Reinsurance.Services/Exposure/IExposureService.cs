using System.Collections.Generic;
using Reinsurance.Services.Exposure.Models;

namespace Reinsurance.Services.Exposure
{
    public partial interface IExposureService
    {
        ExposureSummary GetSummary(int submissionId);
        IList<ExposureRecordModel> GetRecords(int submissionId);
    }
}
