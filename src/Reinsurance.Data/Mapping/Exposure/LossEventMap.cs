using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Exposure;

namespace Reinsurance.Data.Mapping.Exposure
{
    public partial class LossEventMap : EntityTypeConfiguration<LossEvent>
    {
        public LossEventMap()
        {
            ToTable("LossEvents");
            HasKey(x => x.Id);
            Property(x => x.EventName).IsRequired().HasMaxLength(250);
            Property(x => x.GroundUpLoss).HasPrecision(18, 2);
            Property(x => x.CededLoss).HasPrecision(18, 2);
        }
    }
}
