namespace Acme.Workforce.Assets.Tests.Refresh.Review;

using Acme.Workforce.Assets.Refresh.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PrimaryRefreshVarianceStoreChecks
{
    public static void MemoryPrimaryRefreshVarianceStoreFindsWhatItSaved()
    {
        var store = PrimaryRefreshVarianceStore.NewMemoryPrimaryRefreshVarianceStore();
        var item = PrimaryRefreshVariance.NewPrimaryRefreshVariance("code-763", "label-978");
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
        MemoryPrimaryRefreshVarianceStoreFindsWhatItSaved();
    }
}
