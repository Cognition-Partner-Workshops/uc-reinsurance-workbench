using System.Collections.Generic;
using Reinsurance.Core.Domain.CatModel;
using Reinsurance.Core.Domain.Cedents;
using Reinsurance.Core.Domain.Reference;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Core.Domain.Treaties;

namespace Reinsurance.Data.Setup.SeedData
{
    public sealed class SeedState
    {
        public IList<Peril> Perils { get; set; } = new List<Peril>();
        public IList<Region> Regions { get; set; } = new List<Region>();
        public IList<Cedent> Cedents { get; set; } = new List<Cedent>();
        public IList<Broker> Brokers { get; set; } = new List<Broker>();
        public IList<Underwriter> Underwriters { get; set; } = new List<Underwriter>();
        public IList<Submission> Submissions { get; set; } = new List<Submission>();
        public IList<Treaty> Treaties { get; set; } = new List<Treaty>();
        public IList<TreatyLayer> Layers { get; set; } = new List<TreatyLayer>();
        public IList<CatModelResult> CatModels { get; set; } = new List<CatModelResult>();
    }
}
