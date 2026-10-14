namespace Acme.Workforce.Absence.Tests.Balance.Bundle;

using Acme.Workforce.Absence.Balance.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BatchedBalanceOverrideStoreChecks
{
    public static void MemoryBatchedBalanceOverrideStoreFindsWhatItSaved()
    {
        var store = BatchedBalanceOverrideStore.NewMemoryBatchedBalanceOverrideStore();
        var item = BatchedBalanceOverride.NewBatchedBalanceOverride("code-765", "name-953");
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
        MemoryBatchedBalanceOverrideStoreFindsWhatItSaved();
    }
}
