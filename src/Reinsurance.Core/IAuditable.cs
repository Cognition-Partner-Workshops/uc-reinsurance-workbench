using System;

namespace Reinsurance.Core
{
    public interface IAuditable
    {
        DateTime CreatedOnUtc { get; set; }
        DateTime UpdatedOnUtc { get; set; }
    }
}
