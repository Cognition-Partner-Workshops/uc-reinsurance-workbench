using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain.CatModel;
using Reinsurance.Core.Infrastructure;

namespace Reinsurance.Services.CatModel
{
    public partial class CatModelResultService : ICatModelResultService
    {
        private readonly IRepository<CatModelResult> _repository;

        public CatModelResultService(IRepository<CatModelResult> repository)
        {
            Guard.NotNull(repository, nameof(repository));
            _repository = repository;
        }

        public virtual CatModelResult GetLatestPortfolioResult(int submissionId)
        {
            return _repository.Table
                .Where(x => x.SubmissionId == submissionId && x.RegionId == null && x.PerilId == null)
                .OrderByDescending(x => x.RunOn)
                .FirstOrDefault();
        }

        public virtual IList<CatModelResult> GetAll(int submissionId)
        {
            return _repository.Table.Where(x => x.SubmissionId == submissionId)
                .OrderByDescending(x => x.RunOn)
                .ThenBy(x => x.RegionId)
                .ThenBy(x => x.PerilId)
                .ToList();
        }

        public virtual IList<CatModelResult> GetAll(IEnumerable<int> submissionIds)
        {
            Guard.NotNull(submissionIds, nameof(submissionIds));
            var ids = submissionIds.Distinct().ToList();
            if (ids.Count == 0)
                return new List<CatModelResult>();
            return _repository.Table.Where(x => ids.Contains(x.SubmissionId))
                .OrderBy(x => x.SubmissionId)
                .ThenByDescending(x => x.RunOn)
                .ThenBy(x => x.RegionId)
                .ThenBy(x => x.PerilId)
                .ToList();
        }
    }
}
