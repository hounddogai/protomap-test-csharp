namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantMetric;

using Acme.Workforce.Compensation.EquityGrant.GrantMetric;

/// <summary>Checks of grant metric code, run by the project's runner.</summary>
public static class EquityGrantScoreStoreChecks
{
    public static void MemoryEquityGrantScoreStoreFindsWhatItSaved()
    {
        var store = EquityGrantScoreStore.NewMemoryEquityGrantScoreStore();
        var item = EquityGrantScore.NewEquityGrantScore("code-671", "name-682");
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
        MemoryEquityGrantScoreStoreFindsWhatItSaved();
    }
}
