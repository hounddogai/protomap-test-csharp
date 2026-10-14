namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RefreshAdjustmentStoreChecks
{
    public static void MemoryRefreshAdjustmentStoreFindsWhatItSaved()
    {
        var store = RefreshAdjustmentStore.NewMemoryRefreshAdjustmentStore();
        var item = RefreshAdjustment.NewRefreshAdjustment("code-269", "label-790");
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
        MemoryRefreshAdjustmentStoreFindsWhatItSaved();
    }
}
