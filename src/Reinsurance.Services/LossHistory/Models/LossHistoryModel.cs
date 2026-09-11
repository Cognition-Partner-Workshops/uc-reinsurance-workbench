using System;

namespace Reinsurance.Services.LossHistory.Models
{
    public sealed class LossHistoryModel
    {
        public int Id { get; set; }
        public int CedentId { get; set; }
        public string EventName { get; set; }
        public DateTime LossDate { get; set; }
        public decimal GroundUpLoss { get; set; }
        public decimal? CededLoss { get; set; }
    }
}
