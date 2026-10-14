namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshRollupStoreChecks
{
    public static void MemoryRefreshRollupStoreFindsWhatItSaved()
    {
        var store = RefreshRollupStore.NewMemoryRefreshRollupStore();
        var item = RefreshRollup.NewRefreshRollup("code-161", "label-754");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryRefreshRollupStoreFindsWhatItSaved();
    }
}
