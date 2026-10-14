namespace Acme.Workforce.Absence.Tests.Policy.RollingPolicyOverride;

using Acme.Workforce.Absence.Policy.RollingPolicyOverride;

/// <summary>Checks of rolling policy override code, run by the project's runner.</summary>
public static class WeightedPolicyOverrideStoreChecks
{
    public static void MemoryWeightedPolicyOverrideStoreFindsWhatItSaved()
    {
        var store = WeightedPolicyOverrideStore.NewMemoryWeightedPolicyOverrideStore();
        var item = WeightedPolicyOverride.NewWeightedPolicyOverride("code-679", "name-782");
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
        MemoryWeightedPolicyOverrideStoreFindsWhatItSaved();
    }
}
