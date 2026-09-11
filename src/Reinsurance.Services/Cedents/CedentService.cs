using System.Collections.Generic;
using System.Linq;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain.Cedents;
using Reinsurance.Core.Infrastructure;
using Reinsurance.Services.Cedents.Models;

namespace Reinsurance.Services.Cedents
{
    public partial class CedentService : ICedentService
    {
        private readonly IRepository<Cedent> _repository;

        public CedentService(IRepository<Cedent> repository)
        {
            Guard.NotNull(repository, nameof(repository));
            _repository = repository;
        }

        public virtual CedentModel GetById(int cedentId)
        {
            var entity = _repository.GetById(cedentId);
            return entity == null ? null : ToModel(entity);
        }

        public virtual IList<CedentModel> GetAll(bool activeOnly = true)
        {
            var query = _repository.Table;
            if (activeOnly)
                query = query.Where(x => x.Active);
            return query.OrderBy(x => x.Name).ToList().Select(ToModel).ToList();
        }

        public virtual void Insert(CedentModel model)
        {
            Guard.NotNull(model, nameof(model));
            _repository.Insert(new Cedent
            {
                Name = model.Name,
                Code = model.Code,
                Country = model.Country,
                Rating = model.Rating,
                Active = model.Active
            });
        }

        public virtual void Update(CedentModel model)
        {
            Guard.NotNull(model, nameof(model));
            var entity = _repository.GetById(model.Id);
            if (entity == null)
                return;
            entity.Name = model.Name;
            entity.Code = model.Code;
            entity.Country = model.Country;
            entity.Rating = model.Rating;
            entity.Active = model.Active;
            _repository.Update(entity);
        }

        private static CedentModel ToModel(Cedent entity)
        {
            return new CedentModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                Country = entity.Country,
                Rating = entity.Rating,
                Active = entity.Active
            };
        }
    }
}
