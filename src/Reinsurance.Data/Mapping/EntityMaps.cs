using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain;

namespace Reinsurance.Data.Mapping
{
    public abstract class AuditedMap<T> : EntityTypeConfiguration<T> where T : class, Reinsurance.Core.IAuditable
    {
        protected AuditedMap()
        {
            Property(x => x.CreatedOnUtc).IsRequired();
            Property(x => x.UpdatedOnUtc).IsRequired();
        }
    }

    public sealed class CedentMap : AuditedMap<Cedent>
    {
        public CedentMap()
        {
            ToTable("Cedents");
            HasKey(x => x.Id);
            Property(x => x.Name).IsRequired().HasMaxLength(200);
            Property(x => x.Code).IsRequired().HasMaxLength(50);
            Property(x => x.Country).HasMaxLength(100);
            Property(x => x.Rating).HasMaxLength(20);
        }
    }

    public sealed class BrokerMap : EntityTypeConfiguration<Broker>
    {
        public BrokerMap() { ToTable("Brokers"); HasKey(x => x.Id); Property(x => x.Name).IsRequired().HasMaxLength(200); Property(x => x.Code).IsRequired().HasMaxLength(50); }
    }

    public sealed class UnderwriterMap : EntityTypeConfiguration<Underwriter>
    {
        public UnderwriterMap() { ToTable("Underwriters"); HasKey(x => x.Id); Property(x => x.Name).IsRequired().HasMaxLength(200); Property(x => x.Email).HasMaxLength(200); Property(x => x.AuthorityLimit).HasPrecision(18, 2); }
    }

    public sealed class PerilMap : EntityTypeConfiguration<Peril>
    {
        public PerilMap() { ToTable("Perils"); HasKey(x => x.Id); Property(x => x.Code).IsRequired().HasMaxLength(50); Property(x => x.Name).IsRequired().HasMaxLength(200); }
    }

    public sealed class RegionMap : EntityTypeConfiguration<Region>
    {
        public RegionMap() { ToTable("Regions"); HasKey(x => x.Id); Property(x => x.Code).IsRequired().HasMaxLength(50); Property(x => x.Name).IsRequired().HasMaxLength(200); Property(x => x.CountryCode).HasMaxLength(10); }
    }

    public sealed class SubmissionMap : AuditedMap<Submission>
    {
        public SubmissionMap()
        {
            ToTable("Submissions"); HasKey(x => x.Id); Property(x => x.Reference).IsRequired().HasMaxLength(100); Property(x => x.Notes).HasMaxLength(2000);
            HasRequired(x => x.Cedent).WithMany(x => x.Submissions).HasForeignKey(x => x.CedentId).WillCascadeOnDelete(false);
            HasOptional(x => x.Broker).WithMany(x => x.Submissions).HasForeignKey(x => x.BrokerId).WillCascadeOnDelete(false);
            HasOptional(x => x.Underwriter).WithMany(x => x.Submissions).HasForeignKey(x => x.UnderwriterId).WillCascadeOnDelete(false);
        }
    }

    public sealed class TreatyMap : AuditedMap<Treaty>
    {
        public TreatyMap()
        {
            ToTable("Treaties"); HasKey(x => x.Id); Property(x => x.Name).IsRequired().HasMaxLength(200); Property(x => x.Currency).IsRequired().HasMaxLength(10);
            HasRequired(x => x.Submission).WithMany(x => x.Treaties).HasForeignKey(x => x.SubmissionId).WillCascadeOnDelete(false);
        }
    }

    public sealed class TreatyLayerMap : EntityTypeConfiguration<TreatyLayer>
    {
        public TreatyLayerMap()
        {
            ToTable("TreatyLayers"); HasKey(x => x.Id); Property(x => x.Limit).HasPrecision(18, 2); Property(x => x.Attachment).HasPrecision(18, 2); Property(x => x.ReinstatementPremiumPct).HasPrecision(9, 6); Property(x => x.SharePct).HasPrecision(9, 6);
            HasRequired(x => x.Treaty).WithMany(x => x.Layers).HasForeignKey(x => x.TreatyId).WillCascadeOnDelete(false);
        }
    }

    public sealed class ExposureRecordMap : EntityTypeConfiguration<ExposureRecord>
    {
        public ExposureRecordMap()
        {
            ToTable("ExposureRecords"); HasKey(x => x.Id); Property(x => x.TotalInsuredValue).HasPrecision(18, 2); Property(x => x.AverageDeductiblePct).HasPrecision(9, 6);
            HasRequired(x => x.Submission).WithMany(x => x.ExposureRecords).HasForeignKey(x => x.SubmissionId).WillCascadeOnDelete(false);
            HasRequired(x => x.Region).WithMany(x => x.ExposureRecords).HasForeignKey(x => x.RegionId).WillCascadeOnDelete(false);
            HasRequired(x => x.Peril).WithMany(x => x.ExposureRecords).HasForeignKey(x => x.PerilId).WillCascadeOnDelete(false);
        }
    }

    public sealed class LossEventMap : EntityTypeConfiguration<LossEvent>
    {
        public LossEventMap()
        {
            ToTable("LossEvents"); HasKey(x => x.Id); Property(x => x.EventName).IsRequired().HasMaxLength(200); Property(x => x.GroundUpLoss).HasPrecision(18, 2); Property(x => x.CededLoss).HasPrecision(18, 2);
        }
    }

    public sealed class CatModelResultMap : EntityTypeConfiguration<CatModelResult>
    {
        public CatModelResultMap()
        {
            ToTable("CatModelResults"); HasKey(x => x.Id); Property(x => x.AAL).HasPrecision(18, 2); Property(x => x.ExpectedLoss).HasPrecision(18, 2); Property(x => x.PML50).HasPrecision(18, 2); Property(x => x.PML100).HasPrecision(18, 2); Property(x => x.PML250).HasPrecision(18, 2);
            HasRequired(x => x.Submission).WithMany(x => x.CatModelResults).HasForeignKey(x => x.SubmissionId).WillCascadeOnDelete(false);
        }
    }

    public sealed class PricingResultMap : AuditedMap<PricingResult>
    {
        public PricingResultMap()
        {
            ToTable("PricingResults"); HasKey(x => x.Id); Property(x => x.TechnicalPremium).HasPrecision(18, 2); Property(x => x.ExpectedLoss).HasPrecision(18, 2); Property(x => x.ExpenseLoad).HasPrecision(18, 2); Property(x => x.RiskLoad).HasPrecision(18, 2); Property(x => x.RateOnLine).HasPrecision(9, 6); Property(x => x.LossCostPct).HasPrecision(9, 6); Property(x => x.ProfitMarginPct).HasPrecision(9, 6); Property(x => x.ReferralReasons).HasMaxLength(2000);
            HasRequired(x => x.TreatyLayer).WithMany().HasForeignKey(x => x.TreatyLayerId).WillCascadeOnDelete(false);
        }
    }

    public sealed class PortfolioLimitMap : EntityTypeConfiguration<PortfolioLimit>
    {
        public PortfolioLimitMap() { ToTable("PortfolioLimits"); HasKey(x => x.Id); Property(x => x.MaxAggregateLimit).HasPrecision(18, 2); Property(x => x.MaxPML250).HasPrecision(18, 2); Property(x => x.Description).HasMaxLength(200); }
    }

    public sealed class ReferralRuleMap : EntityTypeConfiguration<ReferralRule>
    {
        public ReferralRuleMap() { ToTable("ReferralRules"); HasKey(x => x.Id); Property(x => x.Code).IsRequired().HasMaxLength(50); Property(x => x.Description).IsRequired().HasMaxLength(200); Property(x => x.Threshold).HasPrecision(18, 2); Property(x => x.TextValue).HasMaxLength(100); }
    }
}
