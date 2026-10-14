namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class MeritCycleEstimateStoreChecks
{
    public static void MemoryMeritCycleEstimateStoreFindsWhatItSaved()
    {
        var store = MeritCycleEstimateStore.NewMemoryMeritCycleEstimateStore();
        var item = MeritCycleEstimate.NewMeritCycleEstimate("code-789", "label-453");
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
        MemoryMeritCycleEstimateStoreFindsWhatItSaved();
    }
}
