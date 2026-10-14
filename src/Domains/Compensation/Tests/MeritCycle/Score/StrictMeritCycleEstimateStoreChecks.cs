namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictMeritCycleEstimateStoreChecks
{
    public static void MemoryStrictMeritCycleEstimateStoreFindsWhatItSaved()
    {
        var store = StrictMeritCycleEstimateStore.NewMemoryStrictMeritCycleEstimateStore();
        var item = StrictMeritCycleEstimate.NewStrictMeritCycleEstimate("code-577", "label-745");
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
        MemoryStrictMeritCycleEstimateStoreFindsWhatItSaved();
    }
}
