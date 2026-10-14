namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class WeightedBonusPoolVarianceStoreChecks
{
    public static void MemoryWeightedBonusPoolVarianceStoreFindsWhatItSaved()
    {
        var store = WeightedBonusPoolVarianceStore.NewMemoryWeightedBonusPoolVarianceStore();
        var item = WeightedBonusPoolVariance.NewWeightedBonusPoolVariance("code-936", "title-193");
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
        MemoryWeightedBonusPoolVarianceStoreFindsWhatItSaved();
    }
}
