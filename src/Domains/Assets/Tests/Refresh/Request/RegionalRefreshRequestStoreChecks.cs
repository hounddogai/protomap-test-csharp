namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RegionalRefreshRequestStoreChecks
{
    public static void MemoryRegionalRefreshRequestStoreFindsWhatItSaved()
    {
        var store = RegionalRefreshRequestStore.NewMemoryRegionalRefreshRequestStore();
        var item = RegionalRefreshRequest.NewRegionalRefreshRequest("code-620", "label-996");
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
        MemoryRegionalRefreshRequestStoreFindsWhatItSaved();
    }
}
