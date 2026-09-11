using System;
using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain.Exposure;
using Reinsurance.Core.Infrastructure;
using Reinsurance.Services.LossHistory.Models;

namespace Reinsurance.Services.LossHistory
{
    public partial class LossHistoryService : ILossHistoryService
    {
        private readonly IRepository<LossEvent> _repository;

        public LossHistoryService(IRepository<LossEvent> repository)
        {
            Guard.NotNull(repository, nameof(repository));
            _repository = repository;
        }

        public virtual IList<LossHistoryModel> GetByCedent(int cedentId)
        {
            return _repository.Table.Where(x => x.CedentId == cedentId).OrderByDescending(x => x.LossDate).ToList()
                .Select(x => new LossHistoryModel
                {
                    Id = x.Id,
                    CedentId = cedentId,
                    EventName = x.EventName,
                    LossDate = x.LossDate,
                    GroundUpLoss = x.GroundUpLoss,
                    CededLoss = x.CededLoss
                }).ToList();
        }

        public virtual decimal BurningCost(int cedentId, int years)
        {
            if (years <= 0)
                throw new ArgumentOutOfRangeException(nameof(years));
            var start = DateTime.UtcNow.AddYears(-years);
            var total = _repository.Table.Where(x => x.CedentId == cedentId && x.LossDate >= start)
                .Select(x => (decimal?)x.GroundUpLoss).Sum() ?? 0m;
            return total / years;
        }
    }
}
