namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;

/// <summary>Checks of cycle rollup code, run by the project's runner.</summary>
public static class SeasonalMeritCycleRollupStoreChecks
{
    public static void MemorySeasonalMeritCycleRollupStoreFindsWhatItSaved()
    {
        var store = SeasonalMeritCycleRollupStore.NewMemorySeasonalMeritCycleRollupStore();
        var item = SeasonalMeritCycleRollup.NewSeasonalMeritCycleRollup("code-324", "title-818");
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
        MemorySeasonalMeritCycleRollupStoreFindsWhatItSaved();
    }
}
