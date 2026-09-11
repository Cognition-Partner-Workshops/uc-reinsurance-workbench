using System.Collections.Generic;
using Reinsurance.Services.Treaties.Models;

namespace Reinsurance.Services.Treaties
{
    public partial interface ITreatyService
    {
        TreatyModel GetById(int treatyId);
        IList<TreatyModel> GetBySubmission(int submissionId);
        TreatyModel Create(TreatyCreateRequest request);
        TreatyLayerModel AddLayer(int treatyId, TreatyLayerRequest request);
        TreatyModel Bind(int treatyId);
    }
}
