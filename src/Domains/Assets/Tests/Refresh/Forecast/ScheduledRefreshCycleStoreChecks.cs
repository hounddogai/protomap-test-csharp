namespace Acme.Workforce.Assets.Tests.Refresh.Forecast;

using Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class ScheduledRefreshCycleStoreChecks
{
    public static void MemoryScheduledRefreshCycleStoreFindsWhatItSaved()
    {
        var store = ScheduledRefreshCycleStore.NewMemoryScheduledRefreshCycleStore();
        var item = ScheduledRefreshCycle.NewScheduledRefreshCycle("code-306", "label-989");
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
        MemoryScheduledRefreshCycleStoreFindsWhatItSaved();
    }
}
