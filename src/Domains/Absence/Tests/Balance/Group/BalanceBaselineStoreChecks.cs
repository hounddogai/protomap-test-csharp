namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceBaselineStoreChecks
{
    public static void MemoryBalanceBaselineStoreFindsWhatItSaved()
    {
        var store = BalanceBaselineStore.NewMemoryBalanceBaselineStore();
        var item = BalanceBaseline.NewBalanceBaseline("code-445", "title-479");
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
        MemoryBalanceBaselineStoreFindsWhatItSaved();
    }
}
