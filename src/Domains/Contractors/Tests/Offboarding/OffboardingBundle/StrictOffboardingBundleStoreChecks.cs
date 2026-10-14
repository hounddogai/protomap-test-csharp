namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingBundle;

using Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

/// <summary>Checks of offboarding bundle code, run by the project's runner.</summary>
public static class StrictOffboardingBundleStoreChecks
{
    public static void MemoryStrictOffboardingBundleStoreFindsWhatItSaved()
    {
        var store = StrictOffboardingBundleStore.NewMemoryStrictOffboardingBundleStore();
        var item = StrictOffboardingBundle.NewStrictOffboardingBundle("code-565", "label-572");
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
        MemoryStrictOffboardingBundleStoreFindsWhatItSaved();
    }
}
