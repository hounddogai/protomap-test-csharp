namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class CappedBalanceQuotaStoreChecks
{
    public static void MemoryCappedBalanceQuotaStoreFindsWhatItSaved()
    {
        var store = CappedBalanceQuotaStore.NewMemoryCappedBalanceQuotaStore();
        var item = CappedBalanceQuota.NewCappedBalanceQuota("code-754", "label-896");
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
        MemoryCappedBalanceQuotaStoreFindsWhatItSaved();
    }
}
