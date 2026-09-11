using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Domain.Treaties;

namespace Reinsurance.Services.Referrals
{
    public partial interface IReferralService
    {
        ReferralDecision Evaluate(TreatyLayer layer, PricingResult pricing, Submission submission);
    }
}
