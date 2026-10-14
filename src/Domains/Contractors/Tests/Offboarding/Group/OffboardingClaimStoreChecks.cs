namespace Acme.Workforce.Contractors.Tests.Offboarding.Group;

using Acme.Workforce.Contractors.Offboarding.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class OffboardingClaimStoreChecks
{
    public static void MemoryOffboardingClaimStoreFindsWhatItSaved()
    {
        var store = OffboardingClaimStore.NewMemoryOffboardingClaimStore();
        var item = OffboardingClaim.NewOffboardingClaim("code-556", "name-788");
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
        MemoryOffboardingClaimStoreFindsWhatItSaved();
    }
}
