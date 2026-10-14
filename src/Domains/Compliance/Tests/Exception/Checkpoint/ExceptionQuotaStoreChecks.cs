namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ExceptionQuotaStoreChecks
{
    public static void MemoryExceptionQuotaStoreFindsWhatItSaved()
    {
        var store = ExceptionQuotaStore.NewMemoryExceptionQuotaStore();
        var item = ExceptionQuota.NewExceptionQuota("code-670", "label-269");
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
        MemoryExceptionQuotaStoreFindsWhatItSaved();
    }
}
