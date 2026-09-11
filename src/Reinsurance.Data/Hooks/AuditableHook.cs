using System;
using Reinsurance.Core;
using Reinsurance.Core.Data;
using Reinsurance.Core.Data.Hooks;

namespace Reinsurance.Data.Hooks
{
    public sealed class AuditableHook : IDbSaveHook
    {
        public void OnBeforeSave(IHookedEntity entry)
        {
            var entity = entry.Entity as IAuditable;
            if (entity == null) return;
            var now = DateTime.UtcNow;
            if (entry.InitialState == EntityState.Added)
                entity.CreatedOnUtc = now;
            entity.UpdatedOnUtc = now;
        }

        public void OnAfterSave(IHookedEntity entry) { }
        public void OnBeforeSaveCompleted() { }
        public void OnAfterSaveCompleted() { }
    }
}
