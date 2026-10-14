namespace Acme.Workforce.Orgdesign.Tests.CostModel.Score;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CostModelScoreStoreChecks
{
    public static void MemoryCostModelScoreStoreFindsWhatItSaved()
    {
        var store = CostModelScoreStore.NewMemoryCostModelScoreStore();
        var item = CostModelScore.NewCostModelScore("code-867", "title-688");
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
        MemoryCostModelScoreStoreFindsWhatItSaved();
    }
}
