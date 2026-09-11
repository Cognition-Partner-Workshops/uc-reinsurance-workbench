using System.Collections.Generic;
using Reinsurance.Core.Domain.Reference;
using Reinsurance.Core.Domain.Referrals;

namespace Reinsurance.Services.Reference
{
    public interface IReferenceService
    {
        IList<Peril> GetPerils();
        IList<Region> GetRegions();
        IList<ReferralRule> GetReferralRules();
    }
}
