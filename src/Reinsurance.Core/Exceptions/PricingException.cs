using System;

namespace Reinsurance.Core.Exceptions
{
    public sealed class PricingException : Exception
    {
        public PricingException(string message)
            : base(message)
        {
        }
    }
}
