using System;

namespace Reinsurance.Core.Infrastructure
{
    public static class Guard
    {
        public static void NotNull(object value, string name)
        {
            if (value == null) throw new ArgumentNullException(name);
        }

        public static void NotEmpty(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value cannot be empty.", name);
        }
    }
}
