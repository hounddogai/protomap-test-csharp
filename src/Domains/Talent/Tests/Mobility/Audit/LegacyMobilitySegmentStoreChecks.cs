namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyMobilitySegmentStoreChecks
{
    public static void MemoryLegacyMobilitySegmentStoreFindsWhatItSaved()
    {
        var store = LegacyMobilitySegmentStore.NewMemoryLegacyMobilitySegmentStore();
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("code-28", "title-796");
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
        MemoryLegacyMobilitySegmentStoreFindsWhatItSaved();
    }
}
