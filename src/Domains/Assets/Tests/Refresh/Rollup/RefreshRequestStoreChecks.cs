namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshRequestStoreChecks
{
    public static void MemoryRefreshRequestStoreFindsWhatItSaved()
    {
        var store = RefreshRequestStore.NewMemoryRefreshRequestStore();
        var item = RefreshRequest.NewRefreshRequest("code-482", "title-631");
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
        MemoryRefreshRequestStoreFindsWhatItSaved();
    }
}
