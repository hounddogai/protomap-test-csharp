namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingTierStoreChecks
{
    public static void MemoryOffboardingTierStoreFindsWhatItSaved()
    {
        var store = OffboardingTierStore.NewMemoryOffboardingTierStore();
        var item = OffboardingTier.NewOffboardingTier("code-527", "label-827");
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
        MemoryOffboardingTierStoreFindsWhatItSaved();
    }
}
