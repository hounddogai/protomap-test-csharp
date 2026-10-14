namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PoolSchedule;

/// <summary>Checks of pool schedule code, run by the project's runner.</summary>
public static class BonusPoolScoreStoreChecks
{
    public static void MemoryBonusPoolScoreStoreFindsWhatItSaved()
    {
        var store = BonusPoolScoreStore.NewMemoryBonusPoolScoreStore();
        var item = BonusPoolScore.NewBonusPoolScore("code-228", "title-382");
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
        MemoryBonusPoolScoreStoreFindsWhatItSaved();
    }
}
