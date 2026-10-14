namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingOffboardingRollupStoreChecks
{
    public static void MemoryRollingOffboardingRollupStoreFindsWhatItSaved()
    {
        var store = RollingOffboardingRollupStore.NewMemoryRollingOffboardingRollupStore();
        var item = RollingOffboardingRollup.NewRollingOffboardingRollup("code-880", "label-675");
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
        MemoryRollingOffboardingRollupStoreFindsWhatItSaved();
    }
}
