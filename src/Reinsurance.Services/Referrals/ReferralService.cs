using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain.CatModel;
using Reinsurance.Core.Domain.Cedents;
using Reinsurance.Core.Domain.Exposure;
using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Reference;
using Reinsurance.Core.Domain.Referrals;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Domain.Treaties;
using Reinsurance.Core.Infrastructure;

namespace Reinsurance.Services.Referrals
{
    public partial class ReferralService : IReferralService
    {
        private readonly IRepository<ReferralRule> _rules;
        private readonly IRepository<PortfolioLimit> _limits;
        private readonly IRepository<Underwriter> _underwriters;
        private readonly IRepository<TreatyLayer> _layers;
        private readonly IRepository<ExposureRecord> _exposures;
        private readonly IRepository<CatModelResult> _catModels;
        private readonly IRepository<Region> _regions;
        private readonly IRepository<Peril> _perils;

        public ReferralService(
            IRepository<ReferralRule> rules,
            IRepository<PortfolioLimit> limits,
            IRepository<Underwriter> underwriters,
            IRepository<TreatyLayer> layers,
            IRepository<ExposureRecord> exposures,
            IRepository<CatModelResult> catModels,
            IRepository<Region> regions,
            IRepository<Peril> perils)
        {
            Guard.NotNull(rules, nameof(rules));
            Guard.NotNull(limits, nameof(limits));
            Guard.NotNull(underwriters, nameof(underwriters));
            Guard.NotNull(layers, nameof(layers));
            Guard.NotNull(exposures, nameof(exposures));
            Guard.NotNull(catModels, nameof(catModels));
            Guard.NotNull(regions, nameof(regions));
            Guard.NotNull(perils, nameof(perils));
            _rules = rules;
            _limits = limits;
            _underwriters = underwriters;
            _layers = layers;
            _exposures = exposures;
            _catModels = catModels;
            _regions = regions;
            _perils = perils;
        }

        public virtual ReferralDecision Evaluate(TreatyLayer layer, PricingResult pricing, Submission submission)
        {
            var underwriter = submission.UnderwriterId.HasValue ? _underwriters.GetById(submission.UnderwriterId.Value) : null;
            var exposure = _exposures.Table
                .Where(x => x.SubmissionId == submission.Id)
                .ToList();
            submission.ExposureRecords = exposure;
            var limits = _limits.Table.ToList();
            var boundLayers = _layers.Table
                .Include(x => x.Treaty)
                .Where(x => x.Treaty.Status == TreatyStatus.Bound)
                .ToList();
            var boundSubmissionIds = boundLayers
                .Select(x => x.Treaty.SubmissionId)
                .Distinct()
                .ToList();
            var boundExposure = _exposures.Table
                .Where(x => boundSubmissionIds.Contains(x.SubmissionId))
                .ToList()
                .GroupBy(x => x.SubmissionId)
                .ToDictionary(x => x.Key, x => new
                {
                    Regions = new HashSet<int>(x.Select(y => y.RegionId)),
                    Perils = new HashSet<int>(x.Select(y => y.PerilId))
                });
            var latestPml250 = _catModels.Table
                .Where(x => x.SubmissionId == submission.Id && x.RegionId == null && x.PerilId == null)
                .OrderByDescending(x => x.RunOn)
                .Select(x => (decimal?)x.PML250)
                .FirstOrDefault() ?? 0m;
            var regionCodes = _regions.Table.ToDictionary(x => x.Id, x => x.Code);
            var perilCodes = _perils.Table.ToDictionary(x => x.Id, x => x.Code);
            return new ReferralEngine().Evaluate(
                layer,
                pricing,
                submission,
                underwriter,
                _rules.Table.ToList(),
                limits,
                limit => boundLayers
                    .Where(x => x.Id != layer.Id)
                    .Where(x => boundExposure.ContainsKey(x.Treaty.SubmissionId))
                    .Where(x => ReferralEngine.Matches(
                        limit,
                        boundExposure[x.Treaty.SubmissionId].Regions,
                        boundExposure[x.Treaty.SubmissionId].Perils))
                    .Sum(x => x.Limit),
                latestPml250,
                id => id.HasValue && regionCodes.ContainsKey(id.Value) ? regionCodes[id.Value] : id.ToString(),
                id => id.HasValue && perilCodes.ContainsKey(id.Value) ? perilCodes[id.Value] : id.ToString());
        }
    }
}
