using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain.Reference;
using Reinsurance.Core.Domain.Referrals;
using Reinsurance.Core.Infrastructure;

namespace Reinsurance.Services.Reference
{
    public sealed class ReferenceService : IReferenceService
    {
        private readonly IRepository<Peril> _perils;
        private readonly IRepository<Region> _regions;
        private readonly IRepository<ReferralRule> _rules;

        public ReferenceService(IRepository<Peril> perils, IRepository<Region> regions, IRepository<ReferralRule> rules)
        {
            Guard.NotNull(perils, nameof(perils));
            Guard.NotNull(regions, nameof(regions));
            Guard.NotNull(rules, nameof(rules));
            _perils = perils;
            _regions = regions;
            _rules = rules;
        }

        public IList<Peril> GetPerils() { return _perils.Table.OrderBy(x => x.Code).ToList(); }
        public IList<Region> GetRegions() { return _regions.Table.OrderBy(x => x.Code).ToList(); }
        public IList<ReferralRule> GetReferralRules() { return _rules.Table.OrderBy(x => x.Code).ToList(); }
    }
}
