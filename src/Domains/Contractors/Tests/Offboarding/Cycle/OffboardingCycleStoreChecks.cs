namespace Acme.Workforce.Contractors.Tests.Offboarding.Cycle;

using Acme.Workforce.Contractors.Offboarding.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class OffboardingCycleStoreChecks
{
    public static void MemoryOffboardingCycleStoreFindsWhatItSaved()
    {
        var store = OffboardingCycleStore.NewMemoryOffboardingCycleStore();
        var item = OffboardingCycle.NewOffboardingCycle("code-776", "name-431");
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
        MemoryOffboardingCycleStoreFindsWhatItSaved();
    }
}
