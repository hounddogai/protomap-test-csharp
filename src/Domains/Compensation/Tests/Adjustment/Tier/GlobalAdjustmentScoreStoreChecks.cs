namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GlobalAdjustmentScoreStoreChecks
{
    public static void MemoryGlobalAdjustmentScoreStoreFindsWhatItSaved()
    {
        var store = GlobalAdjustmentScoreStore.NewMemoryGlobalAdjustmentScoreStore();
        var item = GlobalAdjustmentScore.NewGlobalAdjustmentScore("code-717", "title-84");
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
        MemoryGlobalAdjustmentScoreStoreFindsWhatItSaved();
    }
}
