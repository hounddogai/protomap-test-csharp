namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CappedAccrualScoreStoreChecks
{
    public static void MemoryCappedAccrualScoreStoreFindsWhatItSaved()
    {
        var store = CappedAccrualScoreStore.NewMemoryCappedAccrualScoreStore();
        var item = CappedAccrualScore.NewCappedAccrualScore("code-909", "label-351");
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
        MemoryCappedAccrualScoreStoreFindsWhatItSaved();
    }
}
