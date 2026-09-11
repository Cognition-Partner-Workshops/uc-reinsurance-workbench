using System;

namespace Reinsurance.Services.CatModel.Models
{
    public sealed class CatModelResultModel
    {
        public int Id { get; set; }
        public int? RegionId { get; set; }
        public int? PerilId { get; set; }
        public string ModelVendor { get; set; }
        public string ModelVersion { get; set; }
        public decimal AAL { get; set; }
        public decimal ExpectedLoss { get; set; }
        public decimal PML50 { get; set; }
        public decimal PML100 { get; set; }
        public decimal PML250 { get; set; }
        public DateTime RunOn { get; set; }
    }
}
