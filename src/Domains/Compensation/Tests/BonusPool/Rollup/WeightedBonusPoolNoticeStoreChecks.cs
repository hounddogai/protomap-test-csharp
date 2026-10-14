namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedBonusPoolNoticeStoreChecks
{
    public static void MemoryWeightedBonusPoolNoticeStoreFindsWhatItSaved()
    {
        var store = WeightedBonusPoolNoticeStore.NewMemoryWeightedBonusPoolNoticeStore();
        var item = WeightedBonusPoolNotice.NewWeightedBonusPoolNotice("code-855", "name-6");
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
        MemoryWeightedBonusPoolNoticeStoreFindsWhatItSaved();
    }
}
