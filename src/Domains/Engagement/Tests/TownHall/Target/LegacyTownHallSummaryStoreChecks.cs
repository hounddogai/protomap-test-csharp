namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyTownHallSummaryStoreChecks
{
    public static void MemoryLegacyTownHallSummaryStoreFindsWhatItSaved()
    {
        var store = LegacyTownHallSummaryStore.NewMemoryLegacyTownHallSummaryStore();
        var item = LegacyTownHallSummary.NewLegacyTownHallSummary("code-633", "label-517");
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
        MemoryLegacyTownHallSummaryStoreFindsWhatItSaved();
    }
}
