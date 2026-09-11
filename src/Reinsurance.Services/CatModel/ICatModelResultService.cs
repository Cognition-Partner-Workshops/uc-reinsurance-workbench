using System.Collections.Generic;
using Reinsurance.Core.Domain.CatModel;

namespace Reinsurance.Services.CatModel
{
    public partial interface ICatModelResultService
    {
        CatModelResult GetLatestPortfolioResult(int submissionId);
        IList<CatModelResult> GetAll(int submissionId);
    }
}
