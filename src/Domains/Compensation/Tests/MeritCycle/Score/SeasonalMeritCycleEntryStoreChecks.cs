namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SeasonalMeritCycleEntryStoreChecks
{
    public static void MemorySeasonalMeritCycleEntryStoreFindsWhatItSaved()
    {
        var store = SeasonalMeritCycleEntryStore.NewMemorySeasonalMeritCycleEntryStore();
        var item = SeasonalMeritCycleEntry.NewSeasonalMeritCycleEntry("code-217", "label-366");
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
        MemorySeasonalMeritCycleEntryStoreFindsWhatItSaved();
    }
}
