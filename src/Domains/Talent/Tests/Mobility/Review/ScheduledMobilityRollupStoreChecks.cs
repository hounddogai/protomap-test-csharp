namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledMobilityRollupStoreChecks
{
    public static void MemoryScheduledMobilityRollupStoreFindsWhatItSaved()
    {
        var store = ScheduledMobilityRollupStore.NewMemoryScheduledMobilityRollupStore();
        var item = ScheduledMobilityRollup.NewScheduledMobilityRollup("code-650", "title-287");
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
        MemoryScheduledMobilityRollupStoreFindsWhatItSaved();
    }
}
