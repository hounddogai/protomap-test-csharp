namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryRefreshEstimateStoreChecks
{
    public static void MemoryPrimaryRefreshEstimateStoreFindsWhatItSaved()
    {
        var store = PrimaryRefreshEstimateStore.NewMemoryPrimaryRefreshEstimateStore();
        var item = PrimaryRefreshEstimate.NewPrimaryRefreshEstimate("code-525", "label-623");
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
        MemoryPrimaryRefreshEstimateStoreFindsWhatItSaved();
    }
}
