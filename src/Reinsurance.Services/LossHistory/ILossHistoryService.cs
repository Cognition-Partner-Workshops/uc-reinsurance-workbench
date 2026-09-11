using System.Collections.Generic;
using Reinsurance.Services.LossHistory.Models;

namespace Reinsurance.Services.LossHistory
{
    public partial interface ILossHistoryService
    {
        IList<LossHistoryModel> GetByCedent(int cedentId);
        decimal BurningCost(int cedentId, int years);
    }
}
