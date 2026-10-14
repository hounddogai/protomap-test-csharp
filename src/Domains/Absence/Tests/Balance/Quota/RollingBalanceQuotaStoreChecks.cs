namespace Acme.Workforce.Absence.Tests.Balance.Quota;

using Acme.Workforce.Absence.Balance.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RollingBalanceQuotaStoreChecks
{
    public static void MemoryRollingBalanceQuotaStoreFindsWhatItSaved()
    {
        var store = RollingBalanceQuotaStore.NewMemoryRollingBalanceQuotaStore();
        var item = RollingBalanceQuota.NewRollingBalanceQuota("code-773", "title-724");
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
        MemoryRollingBalanceQuotaStoreFindsWhatItSaved();
    }
}
