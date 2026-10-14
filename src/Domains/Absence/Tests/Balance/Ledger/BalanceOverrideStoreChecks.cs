namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceOverrideStoreChecks
{
    public static void MemoryBalanceOverrideStoreFindsWhatItSaved()
    {
        var store = BalanceOverrideStore.NewMemoryBalanceOverrideStore();
        var item = BalanceOverride.NewBalanceOverride("code-757", "name-939");
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
        MemoryBalanceOverrideStoreFindsWhatItSaved();
    }
}
