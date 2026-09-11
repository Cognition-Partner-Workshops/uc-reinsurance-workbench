using System.Collections.Generic;
using System.Linq;

namespace Reinsurance.Core.Data.Hooks
{
    public sealed class DefaultDbHookHandler
    {
        private readonly IList<IDbSaveHook> _hooks;

        public DefaultDbHookHandler(IEnumerable<IDbSaveHook> hooks)
        {
            _hooks = (hooks ?? Enumerable.Empty<IDbSaveHook>()).ToList();
        }

        public void Before(IEnumerable<IHookedEntity> entries)
        {
            foreach (var entry in entries)
                foreach (var hook in _hooks) hook.OnBeforeSave(entry);
            foreach (var hook in _hooks) hook.OnBeforeSaveCompleted();
        }

        public void After(IEnumerable<IHookedEntity> entries)
        {
            foreach (var entry in entries)
                foreach (var hook in _hooks) hook.OnAfterSave(entry);
            foreach (var hook in _hooks) hook.OnAfterSaveCompleted();
        }
    }
}
