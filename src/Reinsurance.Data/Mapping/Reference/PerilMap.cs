using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Reference;

namespace Reinsurance.Data.Mapping.Reference
{
    public partial class PerilMap : EntityTypeConfiguration<Peril>
    {
        public PerilMap()
        {
            ToTable("Perils");
            HasKey(x => x.Id);
            Property(x => x.Code).IsRequired().HasMaxLength(20);
            Property(x => x.Name).IsRequired().HasMaxLength(200);
        }
    }
}
