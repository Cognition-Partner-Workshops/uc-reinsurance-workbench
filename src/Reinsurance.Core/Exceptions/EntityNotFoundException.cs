using System;

namespace Reinsurance.Core.Exceptions
{
    public sealed class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, int id)
            : base(string.Format("{0} with id {1} was not found.", entityName, id))
        {
        }
    }
}
