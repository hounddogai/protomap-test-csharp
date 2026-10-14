namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RollingMeritCycleSegmentStoreChecks
{
    public static void MemoryRollingMeritCycleSegmentStoreFindsWhatItSaved()
    {
        var store = RollingMeritCycleSegmentStore.NewMemoryRollingMeritCycleSegmentStore();
        var item = RollingMeritCycleSegment.NewRollingMeritCycleSegment("code-392", "label-236");
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
        MemoryRollingMeritCycleSegmentStoreFindsWhatItSaved();
    }
}
