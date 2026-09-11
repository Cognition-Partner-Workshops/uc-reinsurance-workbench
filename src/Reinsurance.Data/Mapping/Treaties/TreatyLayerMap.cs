using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Treaties;

namespace Reinsurance.Data.Mapping.Treaties
{
    public partial class TreatyLayerMap : EntityTypeConfiguration<TreatyLayer>
    {
        public TreatyLayerMap()
        {
            ToTable("TreatyLayers");
            HasKey(x => x.Id);
            Property(x => x.Limit).HasPrecision(18, 2);
            Property(x => x.Attachment).HasPrecision(18, 2);
            Property(x => x.ReinstatementPremiumPct).HasPrecision(9, 6);
            Property(x => x.SharePct).HasPrecision(9, 6);
            Property(x => x.Currency).IsRequired().HasMaxLength(10);
            HasRequired(x => x.Treaty).WithMany(x => x.Layers).HasForeignKey(x => x.TreatyId).WillCascadeOnDelete(false);
        }
    }
}
