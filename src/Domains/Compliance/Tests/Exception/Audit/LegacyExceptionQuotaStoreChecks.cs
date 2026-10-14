namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyExceptionQuotaStoreChecks
{
    public static void MemoryLegacyExceptionQuotaStoreFindsWhatItSaved()
    {
        var store = LegacyExceptionQuotaStore.NewMemoryLegacyExceptionQuotaStore();
        var item = LegacyExceptionQuota.NewLegacyExceptionQuota("code-780", "title-762");
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
        MemoryLegacyExceptionQuotaStoreFindsWhatItSaved();
    }
}
