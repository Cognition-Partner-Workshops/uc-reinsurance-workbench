using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Treaties;
using Reinsurance.Core.Exceptions;
using Reinsurance.Core.Infrastructure;
using Reinsurance.Services.CatModel;
using Reinsurance.Services.Referrals;
using Reinsurance.Services.Pricing.Models;
using Sentry;

namespace Reinsurance.Services.Pricing
{
    public partial class PricingService : IPricingService
    {
        private readonly IRepository<TreatyLayer> _layers;
        private readonly IRepository<PricingResult> _pricing;
        private readonly IRepository<Treaty> _treaties;
        private readonly ICatModelResultService _catModels;
        private readonly IReferralService _referrals;

        public PricingService(IRepository<TreatyLayer> layers, IRepository<PricingResult> pricing, IRepository<Treaty> treaties, ICatModelResultService catModels, IReferralService referrals)
        {
            Guard.NotNull(layers, nameof(layers));
            Guard.NotNull(pricing, nameof(pricing));
            Guard.NotNull(treaties, nameof(treaties));
            Guard.NotNull(catModels, nameof(catModels));
            Guard.NotNull(referrals, nameof(referrals));
            _layers = layers;
            _pricing = pricing;
            _treaties = treaties;
            _catModels = catModels;
            _referrals = referrals;
        }

        public virtual PricingResultModel PriceLayer(int treatyLayerId)
        {
            var layer = _layers.Table.Include(x => x.Treaty.Submission.Cedent).FirstOrDefault(x => x.Id == treatyLayerId);
            if (layer == null)
                throw new EntityNotFoundException(nameof(TreatyLayer), treatyLayerId);
            return PriceLayerInternal(layer);
        }

        public virtual IList<PricingResultModel> PriceTreaty(int treatyId)
        {
            var treaty = _treaties.Table.Include(x => x.Layers).Include(x => x.Submission.Cedent).FirstOrDefault(x => x.Id == treatyId);
            if (treaty == null)
                throw new EntityNotFoundException(nameof(Treaty), treatyId);
            return treaty.Layers.OrderBy(x => x.LayerNumber).Select(PriceLayerInternal).ToList();
        }

        public virtual IList<PricingResultModel> GetPricing(int treatyId)
        {
            return _pricing.Table.Where(x => x.TreatyLayer.TreatyId == treatyId).OrderBy(x => x.TreatyLayer.LayerNumber)
                .ToList().Select(x => ToModel(x, x.TreatyLayer.SharePct)).ToList();
        }

        private PricingResultModel PriceLayerInternal(TreatyLayer layer)
        {
            if (SentrySdk.IsEnabled)
            {
                using (SentrySdk.PushScope())
                {
                    SentrySdk.ConfigureScope(scope =>
                    {
                        scope.SetTag("treatyId", layer.TreatyId.ToString());
                        scope.SetTag("layerId", layer.Id.ToString());
                    });
                    return PersistLayer(layer);
                }
            }
            return PersistLayer(layer);
        }

        private PricingResultModel PersistLayer(TreatyLayer layer)
        {
            var model = _catModels.GetLatestPortfolioResult(layer.Treaty.SubmissionId);
            if (model == null)
                throw new PricingException("No portfolio cat model result is available.");
            var output = PricingCalculator.Calculate(new PricingInput
            {
                Limit = layer.Limit,
                Attachment = layer.Attachment,
                SharePct = layer.SharePct,
                Reinstatements = layer.Reinstatements,
                ReinstatementPremiumPct = layer.ReinstatementPremiumPct,
                AAL = model.AAL,
                PML100 = model.PML100,
                PML250 = model.PML250
            });
            var now = DateTime.UtcNow;
            var result = new PricingResult
            {
                TreatyLayerId = layer.Id,
                TechnicalPremium = output.TechnicalPremium,
                ExpectedLoss = output.ExpectedLoss,
                ExpenseLoad = output.ExpenseLoad,
                RiskLoad = output.RiskLoad,
                RateOnLine = output.RateOnLine,
                LossCostPct = output.LossCostPct,
                ProfitMarginPct = output.ProfitMarginPct,
                CalculatedOn = now,
                CreatedOnUtc = now,
                UpdatedOnUtc = now
            };
            var prior = _pricing.Table.FirstOrDefault(x => x.TreatyLayerId == layer.Id);
            if (prior != null)
                _pricing.Delete(prior);
            var decision = _referrals.Evaluate(layer, result, layer.Treaty.Submission);
            result.ReferralRequired = decision.Required;
            result.ReferralReasons = string.Join("; ", decision.Reasons);
            _pricing.Insert(result);
            return ToModel(result, layer.SharePct);
        }

        private static PricingResultModel ToModel(PricingResult result, decimal sharePct)
        {
            return new PricingResultModel
            {
                TreatyLayerId = result.TreatyLayerId,
                TechnicalPremium = result.TechnicalPremium,
                ExpectedLoss = result.ExpectedLoss,
                ExpenseLoad = result.ExpenseLoad,
                RiskLoad = result.RiskLoad,
                RateOnLine = result.RateOnLine,
                LossCostPct = result.LossCostPct,
                ProfitMarginPct = result.ProfitMarginPct,
                ReferralRequired = result.ReferralRequired,
                ReferralReasons = result.ReferralReasons,
                OurShareLine = result.TechnicalPremium * sharePct
            };
        }
    }
}
