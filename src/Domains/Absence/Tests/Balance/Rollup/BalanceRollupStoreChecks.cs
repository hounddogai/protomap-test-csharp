namespace Acme.Workforce.Absence.Tests.Balance.Rollup;

using Acme.Workforce.Absence.Balance.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BalanceRollupStoreChecks
{
    public static void MemoryBalanceRollupStoreFindsWhatItSaved()
    {
        var store = BalanceRollupStore.NewMemoryBalanceRollupStore();
        var item = BalanceRollup.NewBalanceRollup("code-201", "name-206");
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
        MemoryBalanceRollupStoreFindsWhatItSaved();
    }
}
