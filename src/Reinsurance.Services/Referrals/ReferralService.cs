using System.Linq;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain.Cedents;
using Reinsurance.Core.Domain.Pricing;
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

        public ReferralService(IRepository<ReferralRule> rules, IRepository<PortfolioLimit> limits, IRepository<Underwriter> underwriters, IRepository<TreatyLayer> layers)
        {
            Guard.NotNull(rules, nameof(rules));
            Guard.NotNull(limits, nameof(limits));
            Guard.NotNull(underwriters, nameof(underwriters));
            Guard.NotNull(layers, nameof(layers));
            _rules = rules;
            _limits = limits;
            _underwriters = underwriters;
            _layers = layers;
        }

        public virtual ReferralDecision Evaluate(TreatyLayer layer, PricingResult pricing, Submission submission)
        {
            var underwriter = submission.UnderwriterId.HasValue ? _underwriters.GetById(submission.UnderwriterId.Value) : null;
            return new ReferralEngine().Evaluate(layer, pricing, submission, underwriter, _rules.Table.ToList(), _limits.Table.ToList(), _layers.Table.ToList());
        }
    }
}
