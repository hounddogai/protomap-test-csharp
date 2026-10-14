namespace Acme.Workforce.Workplace.Tests.Visitor.Checkpoint;

using Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalVisitorScheduleStoreChecks
{
    public static void MemorySeasonalVisitorScheduleStoreFindsWhatItSaved()
    {
        var store = SeasonalVisitorScheduleStore.NewMemorySeasonalVisitorScheduleStore();
        var item = SeasonalVisitorSchedule.NewSeasonalVisitorSchedule("code-318", "name-537");
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
        MemorySeasonalVisitorScheduleStoreFindsWhatItSaved();
    }
}
