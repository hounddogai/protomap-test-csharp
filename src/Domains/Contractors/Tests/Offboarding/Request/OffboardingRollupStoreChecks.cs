namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingRollupStoreChecks
{
    public static void MemoryOffboardingRollupStoreFindsWhatItSaved()
    {
        var store = OffboardingRollupStore.NewMemoryOffboardingRollupStore();
        var item = OffboardingRollup.NewOffboardingRollup("code-301", "label-554");
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
        MemoryOffboardingRollupStoreFindsWhatItSaved();
    }
}
