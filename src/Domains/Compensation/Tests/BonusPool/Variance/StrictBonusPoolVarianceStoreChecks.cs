namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictBonusPoolVarianceStoreChecks
{
    public static void MemoryStrictBonusPoolVarianceStoreFindsWhatItSaved()
    {
        var store = StrictBonusPoolVarianceStore.NewMemoryStrictBonusPoolVarianceStore();
        var item = StrictBonusPoolVariance.NewStrictBonusPoolVariance("code-411", "label-108");
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
        MemoryStrictBonusPoolVarianceStoreFindsWhatItSaved();
    }
}
