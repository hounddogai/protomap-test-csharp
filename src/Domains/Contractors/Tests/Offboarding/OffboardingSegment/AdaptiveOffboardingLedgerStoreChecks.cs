namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

/// <summary>Checks of offboarding segment code, run by the project's runner.</summary>
public static class AdaptiveOffboardingLedgerStoreChecks
{
    public static void MemoryAdaptiveOffboardingLedgerStoreFindsWhatItSaved()
    {
        var store = AdaptiveOffboardingLedgerStore.NewMemoryAdaptiveOffboardingLedgerStore();
        var item = AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger("code-126", "name-867");
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
        MemoryAdaptiveOffboardingLedgerStoreFindsWhatItSaved();
    }
}
