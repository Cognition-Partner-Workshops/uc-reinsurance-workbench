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
        private static readonly IDictionary<int, decimal> TrendFactors = new Dictionary<int, decimal>
        {
            { 2018, 1.42m },
            { 2019, 1.35m },
            { 2020, 1.28m },
            { 2021, 1.21m },
            { 2022, 1.15m },
            { 2023, 1.10m },
            { 2024, 1.05m },
            { 2025, 1.00m }
        };

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
            var losses = _repository.Table.Where(x => x.CedentId == cedentId && x.LossDate >= start).ToList();
            var trended = losses.Sum(x => x.GroundUpLoss * TrendFactors[x.LossDate.Year]);
            return trended / years;
        }
    }
}
