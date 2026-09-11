using System;
using System.Data.Entity.Infrastructure;

namespace Reinsurance.Core.Data.Hooks
{
    public interface IHookedEntity
    {
        Type ContextType { get; }
        DbEntityEntry Entry { get; }
        BaseEntity Entity { get; }
        EntityState InitialState { get; }
        EntityState State { get; set; }
        bool IsPropertyModified(string propertyName);
    }
}
