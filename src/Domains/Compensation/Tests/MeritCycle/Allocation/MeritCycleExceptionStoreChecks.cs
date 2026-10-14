namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleExceptionStoreChecks
{
    public static void MemoryMeritCycleExceptionStoreFindsWhatItSaved()
    {
        var store = MeritCycleExceptionStore.NewMemoryMeritCycleExceptionStore();
        var item = MeritCycleException.NewMeritCycleException("code-984", "title-950");
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
        MemoryMeritCycleExceptionStoreFindsWhatItSaved();
    }
}
