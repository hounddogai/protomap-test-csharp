namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalNominationCycleStoreChecks
{
    public static void MemorySeasonalNominationCycleStoreFindsWhatItSaved()
    {
        var store = SeasonalNominationCycleStore.NewMemorySeasonalNominationCycleStore();
        var item = SeasonalNominationCycle.NewSeasonalNominationCycle("code-67", "name-857");
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
        MemorySeasonalNominationCycleStoreFindsWhatItSaved();
    }
}
