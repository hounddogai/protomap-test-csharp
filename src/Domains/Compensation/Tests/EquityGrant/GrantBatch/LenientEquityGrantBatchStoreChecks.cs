namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantBatch;

using Acme.Workforce.Compensation.EquityGrant.GrantBatch;

/// <summary>Checks of grant batch code, run by the project's runner.</summary>
public static class LenientEquityGrantBatchStoreChecks
{
    public static void MemoryLenientEquityGrantBatchStoreFindsWhatItSaved()
    {
        var store = LenientEquityGrantBatchStore.NewMemoryLenientEquityGrantBatchStore();
        var item = LenientEquityGrantBatch.NewLenientEquityGrantBatch("code-59", "name-319");
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
        MemoryLenientEquityGrantBatchStoreFindsWhatItSaved();
    }
}
