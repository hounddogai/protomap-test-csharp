namespace Acme.Workforce.Assets.Tests.Refresh.Schedule;

using Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RefreshScheduleStoreChecks
{
    public static void MemoryRefreshScheduleStoreFindsWhatItSaved()
    {
        var store = RefreshScheduleStore.NewMemoryRefreshScheduleStore();
        var item = RefreshSchedule.NewRefreshSchedule("code-942", "title-275");
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
        MemoryRefreshScheduleStoreFindsWhatItSaved();
    }
}
