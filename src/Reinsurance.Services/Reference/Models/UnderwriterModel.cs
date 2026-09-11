namespace Reinsurance.Services.Reference.Models
{
    public sealed class UnderwriterModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal AuthorityLimit { get; set; }
        public bool Active { get; set; }
    }
}
