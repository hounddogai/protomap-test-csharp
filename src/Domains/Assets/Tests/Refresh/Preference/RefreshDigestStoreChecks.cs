namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshDigestStoreChecks
{
    public static void MemoryRefreshDigestStoreFindsWhatItSaved()
    {
        var store = RefreshDigestStore.NewMemoryRefreshDigestStore();
        var item = RefreshDigest.NewRefreshDigest("code-709", "title-906");
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
        MemoryRefreshDigestStoreFindsWhatItSaved();
    }
}
