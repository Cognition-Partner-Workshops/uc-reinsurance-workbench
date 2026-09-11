namespace Reinsurance.Services.Treaties.Models
{
    public sealed class TreatyLayerModel
    {
        public int Id { get; set; }
        public int LayerNumber { get; set; }
        public decimal Limit { get; set; }
        public decimal Attachment { get; set; }
        public int Reinstatements { get; set; }
        public decimal ReinstatementPremiumPct { get; set; }
        public decimal SharePct { get; set; }
        public string Currency { get; set; }
    }
}
