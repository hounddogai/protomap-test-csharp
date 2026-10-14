namespace Acme.Workforce.Contractors.Tests.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PendingOffboardingCycleStoreChecks
{
    public static void MemoryPendingOffboardingCycleStoreFindsWhatItSaved()
    {
        var store = PendingOffboardingCycleStore.NewMemoryPendingOffboardingCycleStore();
        var item = PendingOffboardingCycle.NewPendingOffboardingCycle("code-981", "label-227");
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
        MemoryPendingOffboardingCycleStoreFindsWhatItSaved();
    }
}
