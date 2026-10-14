namespace Acme.Workforce.Orgdesign.Tests.Reorg.Preference;

using Acme.Workforce.Orgdesign.Reorg.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedReorgTargetStoreChecks
{
    public static void MemoryWeightedReorgTargetStoreFindsWhatItSaved()
    {
        var store = WeightedReorgTargetStore.NewMemoryWeightedReorgTargetStore();
        var item = WeightedReorgTarget.NewWeightedReorgTarget("code-324", "label-262");
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
        MemoryWeightedReorgTargetStoreFindsWhatItSaved();
    }
}
