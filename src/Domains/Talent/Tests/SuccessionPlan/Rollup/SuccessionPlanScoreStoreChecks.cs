namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SuccessionPlanScoreStoreChecks
{
    public static void MemorySuccessionPlanScoreStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanScoreStore.NewMemorySuccessionPlanScoreStore();
        var item = SuccessionPlanScore.NewSuccessionPlanScore("code-492", "title-253");
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
        MemorySuccessionPlanScoreStoreFindsWhatItSaved();
    }
}
