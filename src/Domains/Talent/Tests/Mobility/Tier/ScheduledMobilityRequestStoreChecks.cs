namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScheduledMobilityRequestStoreChecks
{
    public static void MemoryScheduledMobilityRequestStoreFindsWhatItSaved()
    {
        var store = ScheduledMobilityRequestStore.NewMemoryScheduledMobilityRequestStore();
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("code-289", "name-650");
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
        MemoryScheduledMobilityRequestStoreFindsWhatItSaved();
    }
}
