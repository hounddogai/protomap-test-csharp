namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class PrimaryPolicyOverrideStoreChecks
{
    public static void MemoryPrimaryPolicyOverrideStoreFindsWhatItSaved()
    {
        var store = PrimaryPolicyOverrideStore.NewMemoryPrimaryPolicyOverrideStore();
        var item = PrimaryPolicyOverride.NewPrimaryPolicyOverride("code-589", "name-224");
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
        MemoryPrimaryPolicyOverrideStoreFindsWhatItSaved();
    }
}
