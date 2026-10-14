namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class WeightedMeritCycleTierStoreChecks
{
    public static void MemoryWeightedMeritCycleTierStoreFindsWhatItSaved()
    {
        var store = WeightedMeritCycleTierStore.NewMemoryWeightedMeritCycleTierStore();
        var item = WeightedMeritCycleTier.NewWeightedMeritCycleTier("code-176", "label-675");
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
        MemoryWeightedMeritCycleTierStoreFindsWhatItSaved();
    }
}
