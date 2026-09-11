using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Cedents;

namespace Reinsurance.Data.Mapping.Cedents
{
    public partial class CedentMap : EntityTypeConfiguration<Cedent>
    {
        public CedentMap()
        {
            ToTable("Cedents");
            HasKey(x => x.Id);
            Property(x => x.Name).IsRequired().HasMaxLength(200);
            Property(x => x.Code).IsRequired().HasMaxLength(50)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UX_Cedents_Code") { IsUnique = true }));
            Property(x => x.Country).HasMaxLength(100);
            Property(x => x.Rating).HasMaxLength(10);
        }
    }
}
