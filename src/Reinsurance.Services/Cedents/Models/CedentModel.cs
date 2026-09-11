namespace Reinsurance.Services.Cedents.Models
{
    public sealed class CedentModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Country { get; set; }
        public string Rating { get; set; }
        public bool Active { get; set; }
    }
}
