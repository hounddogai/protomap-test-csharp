namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingBundleStoreChecks
{
    public static void MemoryOffboardingBundleStoreFindsWhatItSaved()
    {
        var store = OffboardingBundleStore.NewMemoryOffboardingBundleStore();
        var item = OffboardingBundle.NewOffboardingBundle("code-129", "name-918");
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
        MemoryOffboardingBundleStoreFindsWhatItSaved();
    }
}
