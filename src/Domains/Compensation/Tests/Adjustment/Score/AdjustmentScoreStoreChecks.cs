namespace Acme.Workforce.Compensation.Tests.Adjustment.Score;

using Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdjustmentScoreStoreChecks
{
    public static void MemoryAdjustmentScoreStoreFindsWhatItSaved()
    {
        var store = AdjustmentScoreStore.NewMemoryAdjustmentScoreStore();
        var item = AdjustmentScore.NewAdjustmentScore("code-284", "title-374");
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
        MemoryAdjustmentScoreStoreFindsWhatItSaved();
    }
}
