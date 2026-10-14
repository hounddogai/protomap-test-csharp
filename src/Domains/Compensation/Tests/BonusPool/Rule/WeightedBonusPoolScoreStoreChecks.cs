namespace Acme.Workforce.Compensation.Tests.BonusPool.Rule;

using Acme.Workforce.Compensation.BonusPool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class WeightedBonusPoolScoreStoreChecks
{
    public static void MemoryWeightedBonusPoolScoreStoreFindsWhatItSaved()
    {
        var store = WeightedBonusPoolScoreStore.NewMemoryWeightedBonusPoolScoreStore();
        var item = WeightedBonusPoolScore.NewWeightedBonusPoolScore("code-429", "label-208");
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
        MemoryWeightedBonusPoolScoreStoreFindsWhatItSaved();
    }
}
