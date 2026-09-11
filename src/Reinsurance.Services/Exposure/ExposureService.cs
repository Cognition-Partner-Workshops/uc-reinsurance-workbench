using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain.CatModel;
using Reinsurance.Core.Domain.Exposure;
using Reinsurance.Core.Infrastructure;
using Reinsurance.Services.Exposure.Models;

namespace Reinsurance.Services.Exposure
{
    public partial class ExposureService : IExposureService
    {
        private readonly IRepository<ExposureRecord> _repository;
        private readonly IRepository<CatModelResult> _catModels;

        public ExposureService(IRepository<ExposureRecord> repository, IRepository<CatModelResult> catModels)
        {
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(catModels, nameof(catModels));
            _repository = repository;
            _catModels = catModels;
        }

        public virtual ExposureSummary GetSummary(int submissionId)
        {
            var records = _repository.Table.Where(x => x.SubmissionId == submissionId)
                .Include(x => x.Region)
                .Include(x => x.Peril)
                .ToList();
            var model = _catModels.Table.Where(x => x.SubmissionId == submissionId && x.RegionId == null && x.PerilId == null)
                .OrderByDescending(x => x.RunOn)
                .FirstOrDefault();
            return ExposureCalculator.Summarize(records, model == null ? 0m : model.PML250);
        }

        public virtual IDictionary<int, ExposureSummary> GetSummaries(IEnumerable<int> submissionIds)
        {
            Guard.NotNull(submissionIds, nameof(submissionIds));
            var ids = submissionIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<int, ExposureSummary>();
            var records = _repository.Table.Where(x => ids.Contains(x.SubmissionId))
                .Include(x => x.Region)
                .Include(x => x.Peril)
                .ToList()
                .ToLookup(x => x.SubmissionId);
            var models = _catModels.Table.Where(x => ids.Contains(x.SubmissionId) && x.RegionId == null && x.PerilId == null)
                .ToList()
                .GroupBy(x => x.SubmissionId)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.RunOn).First());
            return ids.ToDictionary(
                id => id,
                id => ExposureCalculator.Summarize(records[id], models.ContainsKey(id) ? models[id].PML250 : 0m));
        }

        public virtual IList<ExposureRecordModel> GetRecords(int submissionId)
        {
            return _repository.Table.Where(x => x.SubmissionId == submissionId)
                .Include(x => x.Region)
                .Include(x => x.Peril)
                .OrderBy(x => x.Id)
                .ToList()
                .Select(x => new ExposureRecordModel
                {
                    Id = x.Id,
                    RegionCode = x.Region == null ? null : x.Region.Code,
                    RegionName = x.Region == null ? null : x.Region.Name,
                    PerilCode = x.Peril == null ? null : x.Peril.Code,
                    PerilName = x.Peril == null ? null : x.Peril.Name,
                    TotalInsuredValue = x.TotalInsuredValue,
                    RiskCount = x.RiskCount,
                    AverageDeductiblePct = x.AverageDeductiblePct
                }).ToList();
        }
    }
}
