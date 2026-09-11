using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Reinsurance.Core;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Exceptions;
using Reinsurance.Core.Infrastructure;
using Reinsurance.Services.Exposure;
using Reinsurance.Services.Exposure.Models;
using Reinsurance.Services.Submissions.Models;
using Reinsurance.Services.Treaties.Models;

namespace Reinsurance.Services.Submissions
{
    public partial class SubmissionService : ISubmissionService
    {
        private readonly IRepository<Submission> _repository;
        private readonly IExposureService _exposureService;

        public SubmissionService(IRepository<Submission> repository, IExposureService exposureService)
        {
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(exposureService, nameof(exposureService));
            _repository = repository;
            _exposureService = exposureService;
        }

        public virtual SubmissionModel GetById(int submissionId)
        {
            var entity = Query().FirstOrDefault(x => x.Id == submissionId);
            return entity == null ? null : ToModel(entity);
        }

        public virtual IPagedList<SubmissionListItemModel> Search(SubmissionSearchQuery query)
        {
            Guard.NotNull(query, nameof(query));
            var pageIndex = Math.Max(0, query.PageIndex);
            var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);
            var source = _repository.Table
                .Where(x => !x.Deleted)
                .Include(x => x.Cedent)
                .Include(x => x.Broker)
                .Include(x => x.Underwriter)
                .Include(x => x.Treaties);
            if (query.Status.HasValue)
                source = source.Where(x => x.Status == query.Status.Value);
            if (query.CedentId.HasValue)
                source = source.Where(x => x.CedentId == query.CedentId.Value);
            var total = source.Count();
            var records = source.OrderByDescending(x => x.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();
            var models = records.Select(ToListItemModel).ToList();
            return new PagedList<SubmissionListItemModel>(models, pageIndex, pageSize, total);
        }

        public virtual SubmissionModel Create(SubmissionCreateRequest request)
        {
            Guard.NotNull(request, nameof(request));
            var year = request.InceptionDate == default(DateTime) ? DateTime.UtcNow.Year : request.InceptionDate.Year;
            var prefix = "SUB-" + year + "-";
            var last = _repository.Table.Where(x => x.Reference.StartsWith(prefix))
                .OrderByDescending(x => x.Reference)
                .Select(x => x.Reference)
                .FirstOrDefault();
            var sequence = 1;
            if (!string.IsNullOrEmpty(last) && int.TryParse(last.Substring(prefix.Length), out var current))
                sequence = current + 1;
            var entity = new Submission
            {
                Reference = prefix + sequence.ToString("0000"),
                CedentId = request.CedentId,
                BrokerId = request.BrokerId,
                UnderwriterId = request.UnderwriterId,
                ReceivedOn = DateTime.UtcNow,
                InceptionDate = request.InceptionDate,
                ExpiryDate = request.ExpiryDate,
                Status = SubmissionStatus.Received,
                Notes = request.Notes
            };
            _repository.Insert(entity);
            return GetById(entity.Id);
        }

        public virtual SubmissionModel Transition(int submissionId, SubmissionStatus status)
        {
            var entity = _repository.GetById(submissionId);
            if (entity == null)
                throw new EntityNotFoundException(nameof(Submission), submissionId);
            if (!CanTransition(entity.Status, status))
                throw new InvalidSubmissionTransitionException(entity.Status, status);
            entity.Status = status;
            _repository.Update(entity);
            return GetById(submissionId);
        }

        public virtual void SoftDelete(int submissionId)
        {
            var entity = _repository.GetById(submissionId);
            if (entity == null)
                throw new EntityNotFoundException(nameof(Submission), submissionId);
            entity.Deleted = true;
            _repository.Update(entity);
        }

        public virtual ExposureSummary GetExposureSummary(int submissionId)
        {
            return _exposureService.GetSummary(submissionId);
        }

        public virtual IList<ExposureRecordModel> GetExposureRecords(int submissionId)
        {
            return _exposureService.GetRecords(submissionId);
        }

        private IQueryable<Submission> Query()
        {
            return _repository.Table
                .Where(x => !x.Deleted)
                .Include(x => x.Cedent)
                .Include(x => x.Broker)
                .Include(x => x.Underwriter)
                .Include(x => x.Treaties.Select(y => y.Layers));
        }

        private static bool CanTransition(SubmissionStatus current, SubmissionStatus requested)
        {
            if (requested == SubmissionStatus.Withdrawn && current != SubmissionStatus.Bound && current != SubmissionStatus.Declined && current != SubmissionStatus.Withdrawn)
                return true;
            if (current == SubmissionStatus.Received && requested == SubmissionStatus.InReview)
                return true;
            if (current == SubmissionStatus.InReview && requested == SubmissionStatus.Quoted)
                return true;
            return current == SubmissionStatus.Quoted && (requested == SubmissionStatus.Bound || requested == SubmissionStatus.Declined);
        }

        private static SubmissionModel ToModel(Submission entity)
        {
            return new SubmissionModel
            {
                Id = entity.Id,
                Reference = entity.Reference,
                CedentId = entity.CedentId,
                CedentName = entity.Cedent == null ? null : entity.Cedent.Name,
                CedentRating = entity.Cedent == null ? null : entity.Cedent.Rating,
                BrokerName = entity.Broker == null ? null : entity.Broker.Name,
                UnderwriterName = entity.Underwriter == null ? null : entity.Underwriter.Name,
                Status = entity.Status,
                ReceivedOn = entity.ReceivedOn,
                InceptionDate = entity.InceptionDate,
                ExpiryDate = entity.ExpiryDate,
                Notes = entity.Notes,
                Treaties = entity.Treaties.Select(x => new TreatyModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type,
                    Status = x.Status,
                    Currency = x.Currency,
                    SubmissionId = x.SubmissionId,
                    InceptionDate = x.InceptionDate,
                    ExpiryDate = x.ExpiryDate,
                    Layers = x.Layers.OrderBy(y => y.LayerNumber).Select(y => new TreatyLayerModel
                    {
                        Id = y.Id,
                        LayerNumber = y.LayerNumber,
                        Limit = y.Limit,
                        Attachment = y.Attachment,
                        Reinstatements = y.Reinstatements,
                        ReinstatementPremiumPct = y.ReinstatementPremiumPct,
                        SharePct = y.SharePct,
                        Currency = y.Currency
                    }).ToList()
                }).ToList()
            };
        }

        private static SubmissionListItemModel ToListItemModel(Submission entity)
        {
            return new SubmissionListItemModel
            {
                Id = entity.Id,
                Reference = entity.Reference,
                Status = entity.Status,
                CedentId = entity.CedentId,
                CedentName = entity.Cedent == null ? null : entity.Cedent.Name,
                CedentRating = entity.Cedent == null ? null : entity.Cedent.Rating,
                BrokerName = entity.Broker == null ? null : entity.Broker.Name,
                UnderwriterName = entity.Underwriter == null ? null : entity.Underwriter.Name,
                ReceivedOn = entity.ReceivedOn,
                InceptionDate = entity.InceptionDate,
                ExpiryDate = entity.ExpiryDate,
                TreatyCount = entity.Treaties.Count
            };
        }
    }
}
