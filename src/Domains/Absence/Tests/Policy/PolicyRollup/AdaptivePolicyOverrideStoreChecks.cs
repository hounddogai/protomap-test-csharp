namespace Acme.Workforce.Absence.Tests.Policy.PolicyRollup;

using Acme.Workforce.Absence.Policy.PolicyRollup;

/// <summary>Checks of policy rollup code, run by the project's runner.</summary>
public static class AdaptivePolicyOverrideStoreChecks
{
    public static void MemoryAdaptivePolicyOverrideStoreFindsWhatItSaved()
    {
        var store = AdaptivePolicyOverrideStore.NewMemoryAdaptivePolicyOverrideStore();
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("code-898", "name-580");
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
        MemoryAdaptivePolicyOverrideStoreFindsWhatItSaved();
    }
}
