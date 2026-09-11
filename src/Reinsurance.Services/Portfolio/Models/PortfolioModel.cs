using System.Collections.Generic;
using Reinsurance.Core.Domain.CatModel;
using Reinsurance.Services.Exposure.Models;
using Reinsurance.Services.Pricing.Models;
using Reinsurance.Services.Submissions.Models;

namespace Reinsurance.Services.Portfolio.Models
{
    public sealed class PortfolioModel
    {
        public IList<SubmissionModel> Submissions { get; set; } = new List<SubmissionModel>();
        public IList<TreatyPricingModel> TreatyPricing { get; set; } = new List<TreatyPricingModel>();
        public IList<SubmissionCatModelsModel> CatModels { get; set; } = new List<SubmissionCatModelsModel>();
        public IList<SubmissionExposureModel> Exposures { get; set; } = new List<SubmissionExposureModel>();
    }

    public sealed class TreatyPricingModel
    {
        public int TreatyId { get; set; }
        public IList<PricingResultModel> Results { get; set; } = new List<PricingResultModel>();
    }

    public sealed class SubmissionCatModelsModel
    {
        public int SubmissionId { get; set; }
        public IList<CatModelResult> Results { get; set; } = new List<CatModelResult>();
    }

    public sealed class SubmissionExposureModel
    {
        public int SubmissionId { get; set; }
        public ExposureSummary Summary { get; set; }
    }
}
