namespace Acme.Workforce.Compliance.Tests.Exception.Batch;

using Acme.Workforce.Compliance.Exception.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ExceptionBatchStoreChecks
{
    public static void MemoryExceptionBatchStoreFindsWhatItSaved()
    {
        var store = ExceptionBatchStore.NewMemoryExceptionBatchStore();
        var item = ExceptionBatch.NewExceptionBatch("code-631", "title-8");
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
        MemoryExceptionBatchStoreFindsWhatItSaved();
    }
}
