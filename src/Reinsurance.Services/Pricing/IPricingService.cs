using System.Collections.Generic;
using Reinsurance.Services.Pricing.Models;

namespace Reinsurance.Services.Pricing
{
    public partial interface IPricingService
    {
        PricingResultModel PriceLayer(int treatyLayerId);
        IList<PricingResultModel> PriceTreaty(int treatyId);
        IList<PricingResultModel> GetPricing(int treatyId);
        IDictionary<int, IList<PricingResultModel>> GetPricing(IEnumerable<int> treatyIds);
    }
}
