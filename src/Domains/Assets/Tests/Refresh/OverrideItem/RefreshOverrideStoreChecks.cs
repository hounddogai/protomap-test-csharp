namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RefreshOverrideStoreChecks
{
    public static void MemoryRefreshOverrideStoreFindsWhatItSaved()
    {
        var store = RefreshOverrideStore.NewMemoryRefreshOverrideStore();
        var item = RefreshOverride.NewRefreshOverride("code-616", "label-528");
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
        MemoryRefreshOverrideStoreFindsWhatItSaved();
    }
}
