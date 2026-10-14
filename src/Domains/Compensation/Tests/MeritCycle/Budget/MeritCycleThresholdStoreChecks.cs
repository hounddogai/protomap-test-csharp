namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class MeritCycleThresholdStoreChecks
{
    public static void MemoryMeritCycleThresholdStoreFindsWhatItSaved()
    {
        var store = MeritCycleThresholdStore.NewMemoryMeritCycleThresholdStore();
        var item = MeritCycleThreshold.NewMeritCycleThreshold("code-261", "title-501");
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
        MemoryMeritCycleThresholdStoreFindsWhatItSaved();
    }
}
