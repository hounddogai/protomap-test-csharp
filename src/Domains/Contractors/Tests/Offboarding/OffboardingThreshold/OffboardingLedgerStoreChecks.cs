namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingThreshold;

using Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>Checks of offboarding threshold code, run by the project's runner.</summary>
public static class OffboardingLedgerStoreChecks
{
    public static void MemoryOffboardingLedgerStoreFindsWhatItSaved()
    {
        var store = OffboardingLedgerStore.NewMemoryOffboardingLedgerStore();
        var item = OffboardingLedger.NewOffboardingLedger("code-807", "title-411");
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
        MemoryOffboardingLedgerStoreFindsWhatItSaved();
    }
}
