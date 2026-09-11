using System.Collections.Generic;
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

        public IList<ExposureRecordModel> GetRecords(int submissionId)
        {
            return new List<ExposureRecordModel>();
        }
    }
}
