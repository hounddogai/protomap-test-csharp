namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RefreshProfileStoreChecks
{
    public static void MemoryRefreshProfileStoreFindsWhatItSaved()
    {
        var store = RefreshProfileStore.NewMemoryRefreshProfileStore();
        var item = RefreshProfile.NewRefreshProfile("code-817", "title-285");
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
        MemoryRefreshProfileStoreFindsWhatItSaved();
    }
}
