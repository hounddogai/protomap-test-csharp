namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredAccrualScoreStoreChecks
{
    public static void MemoryTieredAccrualScoreStoreFindsWhatItSaved()
    {
        var store = TieredAccrualScoreStore.NewMemoryTieredAccrualScoreStore();
        var item = TieredAccrualScore.NewTieredAccrualScore("code-160", "title-640");
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
        MemoryTieredAccrualScoreStoreFindsWhatItSaved();
    }
}
