using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain.Cedents;
using Reinsurance.Core.Domain.Reference;
using Reinsurance.Core.Domain.Referrals;
using Reinsurance.Core.Infrastructure;
using Reinsurance.Services.Reference.Models;

namespace Reinsurance.Services.Reference
{
    public sealed class ReferenceService : IReferenceService
    {
        private readonly IRepository<Peril> _perils;
        private readonly IRepository<Region> _regions;
        private readonly IRepository<ReferralRule> _rules;
        private readonly IRepository<Broker> _brokers;
        private readonly IRepository<Underwriter> _underwriters;

        public ReferenceService(
            IRepository<Peril> perils,
            IRepository<Region> regions,
            IRepository<ReferralRule> rules,
            IRepository<Broker> brokers,
            IRepository<Underwriter> underwriters)
        {
            Guard.NotNull(perils, nameof(perils));
            Guard.NotNull(regions, nameof(regions));
            Guard.NotNull(rules, nameof(rules));
            Guard.NotNull(brokers, nameof(brokers));
            Guard.NotNull(underwriters, nameof(underwriters));
            _perils = perils;
            _regions = regions;
            _rules = rules;
            _brokers = brokers;
            _underwriters = underwriters;
        }

        public IList<Peril> GetPerils() { return _perils.Table.OrderBy(x => x.Code).ToList(); }
        public IList<Region> GetRegions() { return _regions.Table.OrderBy(x => x.Code).ToList(); }
        public IList<ReferralRule> GetReferralRules() { return _rules.Table.OrderBy(x => x.Code).ToList(); }

        public IList<BrokerModel> GetBrokers()
        {
            return _brokers.Table
                .OrderBy(x => x.Name)
                .ToList()
                .Select(x => new BrokerModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    Active = true
                })
                .ToList();
        }

        public IList<UnderwriterModel> GetUnderwriters()
        {
            return _underwriters.Table
                .OrderBy(x => x.Name)
                .ToList()
                .Select(x => new UnderwriterModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    AuthorityLimit = x.AuthorityLimit,
                    Active = x.Active
                })
                .ToList();
        }
    }
}
