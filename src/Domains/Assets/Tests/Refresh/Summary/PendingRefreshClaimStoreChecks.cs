namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingRefreshClaimStoreChecks
{
    public static void MemoryPendingRefreshClaimStoreFindsWhatItSaved()
    {
        var store = PendingRefreshClaimStore.NewMemoryPendingRefreshClaimStore();
        var item = PendingRefreshClaim.NewPendingRefreshClaim("code-247", "title-488");
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
        MemoryPendingRefreshClaimStoreFindsWhatItSaved();
    }
}
