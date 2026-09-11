using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Reinsurance.Core.Data;
using Reinsurance.Core.Domain;
using Reinsurance.Services.DTOs;
using Reinsurance.Services.Pricing;
using Reinsurance.Services.Referrals;

namespace Reinsurance.Services
{
    public interface ICedentService { IList<CedentDto> GetAll(); CedentDto Get(int id); }
    public interface ISubmissionService { IList<SubmissionDto> GetAll(); SubmissionDto Get(int id); SubmissionDto Create(SubmissionCreateDto dto); SubmissionDto Transition(int id, TransitionDto dto); IList<ExposureDto> Exposure(int id); IList<CatModelDto> CatModel(int id); }
    public interface ITreatyService { TreatyDto Get(int id); TreatyDto Create(TreatyCreateDto dto); IList<PricingResultDto> Price(int id); TreatyDto Bind(int id); IList<PricingResultDto> Pricing(int id); }
    public interface IReferenceService { IList<Peril> Perils(); IList<Region> Regions(); IList<ReferralRuleDto> ReferralRules(); }
    public interface ILossHistoryService { IList<LossHistoryDto> Get(int cedentId); }

    public sealed class CedentService : ICedentService
    {
        private readonly IRepository<Cedent> _repo;
        public CedentService(IRepository<Cedent> repo) { _repo = repo; }
        public IList<CedentDto> GetAll() { return _repo.Table.OrderBy(x => x.Name).Select(x => new CedentDto { Id = x.Id, Name = x.Name, Code = x.Code, Country = x.Country, Rating = x.Rating, Active = x.Active }).ToList(); }
        public CedentDto Get(int id) { return GetAll().FirstOrDefault(x => x.Id == id); }
    }

    public sealed class SubmissionService : ISubmissionService
    {
        private readonly IRepository<Submission> _repo;
        private readonly IRepository<ExposureRecord> _exposures;
        private readonly IRepository<CatModelResult> _catModels;
        public SubmissionService(IRepository<Submission> repo, IRepository<ExposureRecord> exposures, IRepository<CatModelResult> catModels) { _repo = repo; _exposures = exposures; _catModels = catModels; }
        public IList<SubmissionDto> GetAll() { return Query().OrderBy(x => x.Id).ToList(); }
        public SubmissionDto Get(int id) { return Query().FirstOrDefault(x => x.Id == id); }
        public SubmissionDto Create(SubmissionCreateDto dto)
        {
            var entity = new Submission { Reference = dto.Reference, CedentId = dto.CedentId, BrokerId = dto.BrokerId, UnderwriterId = dto.UnderwriterId, ReceivedOn = DateTime.UtcNow, InceptionDate = dto.InceptionDate, ExpiryDate = dto.ExpiryDate, Notes = dto.Notes, Status = SubmissionStatus.Received };
            _repo.Insert(entity);
            return Get(entity.Id);
        }
        public SubmissionDto Transition(int id, TransitionDto dto) { var entity = _repo.GetById(id); if (entity == null) return null; entity.Status = dto.Status; _repo.Update(entity); return Get(id); }
        public IList<ExposureDto> Exposure(int id) { return _exposures.Table.Where(x => x.SubmissionId == id).Include(x => x.Region).Include(x => x.Peril).Select(x => new ExposureDto { Id = x.Id, Region = x.Region.Name, Peril = x.Peril.Name, TotalInsuredValue = x.TotalInsuredValue, RiskCount = x.RiskCount, AverageDeductiblePct = x.AverageDeductiblePct }).ToList(); }
        public IList<CatModelDto> CatModel(int id) { return _catModels.Table.Where(x => x.SubmissionId == id).OrderBy(x => x.RegionId).ThenBy(x => x.PerilId).Select(x => new CatModelDto { Id = x.Id, RegionId = x.RegionId, PerilId = x.PerilId, ModelVendor = x.ModelVendor, ModelVersion = x.ModelVersion, AAL = x.AAL, ExpectedLoss = x.ExpectedLoss, PML50 = x.PML50, PML100 = x.PML100, PML250 = x.PML250, RunOn = x.RunOn }).ToList(); }
        private IQueryable<SubmissionDto> Query() { return _repo.Table.Where(x => !x.Deleted).Include(x => x.Cedent).Include(x => x.Broker).Include(x => x.Underwriter).Select(x => new SubmissionDto { Id = x.Id, Reference = x.Reference, Cedent = x.Cedent.Name, Broker = x.Broker.Name, Underwriter = x.Underwriter.Name, Status = x.Status, InceptionDate = x.InceptionDate, ExpiryDate = x.ExpiryDate, Notes = x.Notes }); }
    }

    public sealed class TreatyService : ITreatyService
    {
        private readonly IRepository<Treaty> _repo;
        private readonly IRepository<CatModelResult> _cat;
        private readonly IRepository<PricingResult> _pricing;
        private readonly IRepository<ReferralRule> _rules;
        private readonly IRepository<Underwriter> _underwriters;
        private readonly IDbContext _context;
        public TreatyService(IRepository<Treaty> repo, IRepository<CatModelResult> cat, IRepository<PricingResult> pricing, IRepository<ReferralRule> rules, IRepository<Underwriter> underwriters, IDbContext context) { _repo = repo; _cat = cat; _pricing = pricing; _rules = rules; _underwriters = underwriters; _context = context; }
        public TreatyDto Get(int id) { return ToDto(_repo.Table.Include(x => x.Layers).FirstOrDefault(x => x.Id == id)); }
        public TreatyDto Create(TreatyCreateDto dto) { var entity = new Treaty { SubmissionId = dto.SubmissionId, Name = dto.Name, Type = dto.Type, Currency = dto.Currency, InceptionDate = dto.InceptionDate, ExpiryDate = dto.ExpiryDate, Status = TreatyStatus.Draft }; _repo.Insert(entity); return Get(entity.Id); }
        public TreatyDto Bind(int id) { var treaty = _repo.GetById(id); if (treaty == null) return null; treaty.Status = TreatyStatus.Bound; _repo.Update(treaty); return Get(id); }
        public IList<PricingResultDto> Pricing(int id) { return _pricing.Table.Where(x => x.TreatyLayer.TreatyId == id).Select(x => new PricingResultDto { TreatyLayerId = x.TreatyLayerId, TechnicalPremium = x.TechnicalPremium, ExpectedLoss = x.ExpectedLoss, ExpenseLoad = x.ExpenseLoad, RiskLoad = x.RiskLoad, RateOnLine = x.RateOnLine, LossCostPct = x.LossCostPct, ProfitMarginPct = x.ProfitMarginPct, ReferralRequired = x.ReferralRequired, ReferralReasons = x.ReferralReasons, OurShareLine = x.TechnicalPremium * x.TreatyLayer.SharePct }).ToList(); }
        public IList<PricingResultDto> Price(int id)
        {
            var treaty = _repo.Table.Include(x => x.Layers).Include(x => x.Submission).FirstOrDefault(x => x.Id == id);
            if (treaty == null) return null;
            var model = _cat.Table.Where(x => x.SubmissionId == treaty.SubmissionId && x.RegionId == null && x.PerilId == null).OrderByDescending(x => x.RunOn).FirstOrDefault();
            if (model == null) throw new InvalidOperationException("No portfolio cat model result is available.");
            var underwriter = _underwriters.GetById(treaty.Submission.UnderwriterId ?? 0);
            var result = new List<PricingResult>();
            foreach (var layer in treaty.Layers)
            {
                PricingOutput output;
                try
                {
                    output = PricingCalculator.Calculate(new PricingInput { Limit = layer.Limit, Attachment = layer.Attachment, SharePct = layer.SharePct, Reinstatements = layer.Reinstatements, ReinstatementPremiumPct = layer.ReinstatementPremiumPct, AAL = model.AAL, PML100 = model.PML100, PML250 = model.PML250 });
                }
                catch (DivideByZeroException ex)
                {
                    throw new OverflowException("Pricing calculation produced an invalid layer hit fraction.", ex);
                }
                var now = DateTime.UtcNow;
                var pricing = new PricingResult { TreatyLayerId = layer.Id, TechnicalPremium = output.TechnicalPremium, ExpectedLoss = output.ExpectedLoss, ExpenseLoad = output.ExpenseLoad, RiskLoad = output.RiskLoad, RateOnLine = output.RateOnLine, LossCostPct = output.LossCostPct, ProfitMarginPct = output.ProfitMarginPct, CalculatedOn = now, CreatedOnUtc = now, UpdatedOnUtc = now };
                var decision = new ReferralEngine().Evaluate(layer, pricing, underwriter, _rules.Table.ToList());
                pricing.ReferralRequired = decision.Required;
                pricing.ReferralReasons = string.Join("; ", decision.Reasons);
                result.Add(pricing);
            }
            foreach (var existing in _pricing.Table.Where(x => x.TreatyLayer.TreatyId == id).ToList()) _pricing.Delete(existing);
            foreach (var item in result) _pricing.Insert(item);
            return result.Select(x => new PricingResultDto { TreatyLayerId = x.TreatyLayerId, TechnicalPremium = x.TechnicalPremium, ExpectedLoss = x.ExpectedLoss, ExpenseLoad = x.ExpenseLoad, RiskLoad = x.RiskLoad, RateOnLine = x.RateOnLine, LossCostPct = x.LossCostPct, ProfitMarginPct = x.ProfitMarginPct, ReferralRequired = x.ReferralRequired, ReferralReasons = x.ReferralReasons, OurShareLine = x.TechnicalPremium * treaty.Layers.First(y => y.Id == x.TreatyLayerId).SharePct }).ToList();
        }
        private static TreatyDto ToDto(Treaty x) { return x == null ? null : new TreatyDto { Id = x.Id, Name = x.Name, Type = x.Type, Currency = x.Currency, Status = x.Status, SubmissionId = x.SubmissionId, Layers = x.Layers.OrderBy(y => y.LayerNumber).Select(y => new LayerDto { Id = y.Id, LayerNumber = y.LayerNumber, Limit = y.Limit, Attachment = y.Attachment, Reinstatements = y.Reinstatements, ReinstatementPremiumPct = y.ReinstatementPremiumPct, SharePct = y.SharePct, Currency = y.Currency }).ToList() }; }
    }

    public sealed class ReferenceService : IReferenceService
    {
        private readonly IRepository<Peril> _perils; private readonly IRepository<Region> _regions; private readonly IRepository<ReferralRule> _rules;
        public ReferenceService(IRepository<Peril> perils, IRepository<Region> regions, IRepository<ReferralRule> rules) { _perils = perils; _regions = regions; _rules = rules; }
        public IList<Peril> Perils() { return _perils.Table.OrderBy(x => x.Code).ToList(); }
        public IList<Region> Regions() { return _regions.Table.OrderBy(x => x.Code).ToList(); }
        public IList<ReferralRuleDto> ReferralRules() { return _rules.Table.OrderBy(x => x.Code).Select(x => new ReferralRuleDto { Id = x.Id, Code = x.Code, Description = x.Description, Active = x.Active, Kind = x.Kind, Threshold = x.Threshold, TextValue = x.TextValue }).ToList(); }
    }

    public sealed class LossHistoryService : ILossHistoryService
    {
        private readonly IRepository<LossEvent> _repo;
        public LossHistoryService(IRepository<LossEvent> repo) { _repo = repo; }
        public IList<LossHistoryDto> Get(int cedentId) { return _repo.Table.Where(x => x.CedentId == cedentId).OrderByDescending(x => x.LossDate).Select(x => new LossHistoryDto { Id = x.Id, CedentId = x.CedentId, EventName = x.EventName, LossDate = x.LossDate, GroundUpLoss = x.GroundUpLoss, CededLoss = x.CededLoss }).ToList(); }
    }
}
