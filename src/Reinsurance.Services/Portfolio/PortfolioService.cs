using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Infrastructure;
using Reinsurance.Services.CatModel;
using Reinsurance.Services.Exposure;
using Reinsurance.Services.Portfolio.Models;
using Reinsurance.Services.Pricing;
using Reinsurance.Services.Submissions;

namespace Reinsurance.Services.Portfolio
{
    public partial class PortfolioService : IPortfolioService
    {
        private readonly ISubmissionService _submissions;
        private readonly IPricingService _pricing;
        private readonly ICatModelResultService _catModels;
        private readonly IExposureService _exposure;

        public PortfolioService(
            ISubmissionService submissions,
            IPricingService pricing,
            ICatModelResultService catModels,
            IExposureService exposure)
        {
            Guard.NotNull(submissions, nameof(submissions));
            Guard.NotNull(pricing, nameof(pricing));
            Guard.NotNull(catModels, nameof(catModels));
            Guard.NotNull(exposure, nameof(exposure));
            _submissions = submissions;
            _pricing = pricing;
            _catModels = catModels;
            _exposure = exposure;
        }

        public virtual PortfolioModel Get()
        {
            var submissions = _submissions.GetAll();
            var submissionIds = submissions.Select(x => x.Id).ToList();
            var treatyIds = submissions.SelectMany(x => x.Treaties).Select(x => x.Id).ToList();
            var pricing = _pricing.GetPricing(treatyIds);
            var catModels = _catModels.GetAll(submissionIds).ToLookup(x => x.SubmissionId);
            var exposures = _exposure.GetSummaries(submissionIds);
            return new PortfolioModel
            {
                Submissions = submissions,
                TreatyPricing = treatyIds.Select(id => new TreatyPricingModel
                {
                    TreatyId = id,
                    Results = pricing.ContainsKey(id) ? pricing[id] : new List<Pricing.Models.PricingResultModel>()
                }).ToList(),
                CatModels = submissionIds.Select(id => new SubmissionCatModelsModel
                {
                    SubmissionId = id,
                    Results = catModels[id].ToList()
                }).ToList(),
                Exposures = submissionIds.Select(id => new SubmissionExposureModel
                {
                    SubmissionId = id,
                    Summary = exposures.ContainsKey(id) ? exposures[id] : new Exposure.Models.ExposureSummary()
                }).ToList()
            };
        }
    }
}
