namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleTargetStoreChecks
{
    public static void MemoryMeritCycleTargetStoreFindsWhatItSaved()
    {
        var store = MeritCycleTargetStore.NewMemoryMeritCycleTargetStore();
        var item = MeritCycleTarget.NewMeritCycleTarget("code-101", "name-67");
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
        MemoryMeritCycleTargetStoreFindsWhatItSaved();
    }
}
