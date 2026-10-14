namespace Acme.Workforce.Assets.Tests.Refresh.RefreshRequest;

using Acme.Workforce.Assets.Refresh.RefreshRequest;

/// <summary>Checks of refresh request code, run by the project's runner.</summary>
public static class GlobalRefreshOverrideStoreChecks
{
    public static void MemoryGlobalRefreshOverrideStoreFindsWhatItSaved()
    {
        var store = GlobalRefreshOverrideStore.NewMemoryGlobalRefreshOverrideStore();
        var item = GlobalRefreshOverride.NewGlobalRefreshOverride("code-486", "name-402");
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
        MemoryGlobalRefreshOverrideStoreFindsWhatItSaved();
    }
}
