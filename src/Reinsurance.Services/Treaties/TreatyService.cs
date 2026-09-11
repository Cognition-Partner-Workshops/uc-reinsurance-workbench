using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain.Pricing;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Domain.Treaties;
using Reinsurance.Core.Exceptions;
using Reinsurance.Core.Infrastructure;
using Reinsurance.Services.Pricing;
using Reinsurance.Services.Treaties.Models;

namespace Reinsurance.Services.Treaties
{
    public partial class TreatyService : ITreatyService
    {
        private readonly IRepository<Treaty> _repository;
        private readonly IRepository<PricingResult> _pricingRepository;

        public TreatyService(IRepository<Treaty> repository, IRepository<PricingResult> pricingRepository)
        {
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(pricingRepository, nameof(pricingRepository));
            _repository = repository;
            _pricingRepository = pricingRepository;
        }

        public virtual TreatyModel GetById(int treatyId)
        {
            var entity = Query().FirstOrDefault(x => x.Id == treatyId);
            return entity == null ? null : ToModel(entity);
        }

        public virtual IList<TreatyModel> GetBySubmission(int submissionId)
        {
            return Query().Where(x => x.SubmissionId == submissionId).OrderBy(x => x.Id).ToList().Select(ToModel).ToList();
        }

        public virtual TreatyModel Create(TreatyCreateRequest request)
        {
            Guard.NotNull(request, nameof(request));
            var entity = new Treaty
            {
                SubmissionId = request.SubmissionId,
                Name = request.Name,
                Type = request.Type,
                Currency = request.Currency,
                Status = TreatyStatus.Draft,
                InceptionDate = request.InceptionDate,
                ExpiryDate = request.ExpiryDate
            };
            foreach (var layer in request.Layers ?? new List<TreatyLayerRequest>())
                entity.Layers.Add(ToEntity(layer));
            _repository.Insert(entity);
            return GetById(entity.Id);
        }

        public virtual TreatyLayerModel AddLayer(int treatyId, TreatyLayerRequest request)
        {
            Guard.NotNull(request, nameof(request));
            var treaty = Query().FirstOrDefault(x => x.Id == treatyId);
            if (treaty == null)
                throw new EntityNotFoundException(nameof(Treaty), treatyId);
            var layer = ToEntity(request);
            treaty.Layers.Add(layer);
            _repository.Update(treaty);
            return ToLayerModel(layer);
        }

        public virtual TreatyModel Bind(int treatyId)
        {
            var treaty = Query().FirstOrDefault(x => x.Id == treatyId);
            if (treaty == null)
                throw new EntityNotFoundException(nameof(Treaty), treatyId);
            var results = treaty.Layers.Select(x => _pricingRepository.Table.FirstOrDefault(y => y.TreatyLayerId == x.Id)).ToList();
            var referrals = results.Count(x => x == null || x.ReferralRequired);
            if (referrals > 0)
                throw new PricingException(string.Format("Treaty cannot be bound: {0} layer(s) require referral or are unpriced", referrals));
            treaty.Status = TreatyStatus.Bound;
            treaty.Submission.Status = SubmissionStatus.Bound;
            _repository.Update(treaty);
            return GetById(treatyId);
        }

        private IQueryable<Treaty> Query()
        {
            return _repository.Table.Include(x => x.Layers).Include(x => x.Submission);
        }

        private static TreatyLayer ToEntity(TreatyLayerRequest request)
        {
            return new TreatyLayer
            {
                LayerNumber = request.LayerNumber,
                Limit = request.Limit,
                Attachment = request.Attachment,
                Reinstatements = request.Reinstatements,
                ReinstatementPremiumPct = request.ReinstatementPremiumPct,
                SharePct = request.SharePct,
                Currency = request.Currency
            };
        }

        private static TreatyModel ToModel(Treaty entity)
        {
            return new TreatyModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type,
                Status = entity.Status,
                Currency = entity.Currency,
                SubmissionId = entity.SubmissionId,
                InceptionDate = entity.InceptionDate,
                ExpiryDate = entity.ExpiryDate,
                Layers = entity.Layers.OrderBy(x => x.LayerNumber).Select(ToLayerModel).ToList()
            };
        }

        private static TreatyLayerModel ToLayerModel(TreatyLayer entity)
        {
            return new TreatyLayerModel
            {
                Id = entity.Id,
                LayerNumber = entity.LayerNumber,
                Limit = entity.Limit,
                Attachment = entity.Attachment,
                Reinstatements = entity.Reinstatements,
                ReinstatementPremiumPct = entity.ReinstatementPremiumPct,
                SharePct = entity.SharePct,
                Currency = entity.Currency
            };
        }
    }
}
