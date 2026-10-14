namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveVisitorCycleStoreChecks
{
    public static void MemoryAdaptiveVisitorCycleStoreFindsWhatItSaved()
    {
        var store = AdaptiveVisitorCycleStore.NewMemoryAdaptiveVisitorCycleStore();
        var item = AdaptiveVisitorCycle.NewAdaptiveVisitorCycle("code-891", "title-896");
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
        MemoryAdaptiveVisitorCycleStoreFindsWhatItSaved();
    }
}
