using Reinsurance.Core;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Services.Exposure.Models;
using Reinsurance.Services.Submissions.Models;

namespace Reinsurance.Services.Submissions
{
    public partial interface ISubmissionService
    {
        SubmissionModel GetById(int submissionId);
        IPagedList<Submission> Search(SubmissionSearchQuery query);
        SubmissionModel Create(SubmissionCreateRequest request);
        SubmissionModel Transition(int submissionId, SubmissionStatus status);
        void SoftDelete(int submissionId);
        ExposureSummary GetExposureSummary(int submissionId);
        System.Collections.Generic.IList<ExposureRecordModel> GetExposureRecords(int submissionId);
    }
}
