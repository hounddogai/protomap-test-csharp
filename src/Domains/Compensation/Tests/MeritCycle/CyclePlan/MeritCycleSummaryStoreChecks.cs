namespace Acme.Workforce.Compensation.Tests.MeritCycle.CyclePlan;

using Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>Checks of cycle plan code, run by the project's runner.</summary>
public static class MeritCycleSummaryStoreChecks
{
    public static void MemoryMeritCycleSummaryStoreFindsWhatItSaved()
    {
        var store = MeritCycleSummaryStore.NewMemoryMeritCycleSummaryStore();
        var item = MeritCycleSummary.NewMeritCycleSummary("code-839", "label-237");
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
        MemoryMeritCycleSummaryStoreFindsWhatItSaved();
    }
}
