namespace Acme.Workforce.Compensation.Tests.EquityGrant.Quota;

using Acme.Workforce.Compensation.EquityGrant.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScheduledEquityGrantQuotaStoreChecks
{
    public static void MemoryScheduledEquityGrantQuotaStoreFindsWhatItSaved()
    {
        var store = ScheduledEquityGrantQuotaStore.NewMemoryScheduledEquityGrantQuotaStore();
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("code-60", "title-629");
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
        MemoryScheduledEquityGrantQuotaStoreFindsWhatItSaved();
    }
}
