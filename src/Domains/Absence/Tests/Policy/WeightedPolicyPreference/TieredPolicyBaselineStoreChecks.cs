namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class TieredPolicyBaselineStoreChecks
{
    public static void MemoryTieredPolicyBaselineStoreFindsWhatItSaved()
    {
        var store = TieredPolicyBaselineStore.NewMemoryTieredPolicyBaselineStore();
        var item = TieredPolicyBaseline.NewTieredPolicyBaseline("code-785", "title-94");
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
        MemoryTieredPolicyBaselineStoreFindsWhatItSaved();
    }
}
