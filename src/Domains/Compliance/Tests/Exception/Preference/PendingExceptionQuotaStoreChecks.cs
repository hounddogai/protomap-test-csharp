namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingExceptionQuotaStoreChecks
{
    public static void MemoryPendingExceptionQuotaStoreFindsWhatItSaved()
    {
        var store = PendingExceptionQuotaStore.NewMemoryPendingExceptionQuotaStore();
        var item = PendingExceptionQuota.NewPendingExceptionQuota("code-599", "name-932");
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
        MemoryPendingExceptionQuotaStoreFindsWhatItSaved();
    }
}
