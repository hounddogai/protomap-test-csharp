namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class WeightedMeritCycleSegmentStoreChecks
{
    public static void MemoryWeightedMeritCycleSegmentStoreFindsWhatItSaved()
    {
        var store = WeightedMeritCycleSegmentStore.NewMemoryWeightedMeritCycleSegmentStore();
        var item = WeightedMeritCycleSegment.NewWeightedMeritCycleSegment("code-441", "title-829");
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
        MemoryWeightedMeritCycleSegmentStoreFindsWhatItSaved();
    }
}
