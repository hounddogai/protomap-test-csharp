namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictSuccessionPlanScoreStoreChecks
{
    public static void MemoryStrictSuccessionPlanScoreStoreFindsWhatItSaved()
    {
        var store = StrictSuccessionPlanScoreStore.NewMemoryStrictSuccessionPlanScoreStore();
        var item = StrictSuccessionPlanScore.NewStrictSuccessionPlanScore("code-121", "name-150");
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
        MemoryStrictSuccessionPlanScoreStoreFindsWhatItSaved();
    }
}
