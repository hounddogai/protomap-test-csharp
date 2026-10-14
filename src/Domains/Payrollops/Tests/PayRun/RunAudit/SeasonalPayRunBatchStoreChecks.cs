namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAudit;

using Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Checks of run audit code, run by the project's runner.</summary>
public static class SeasonalPayRunBatchStoreChecks
{
    public static void MemorySeasonalPayRunBatchStoreFindsWhatItSaved()
    {
        var store = SeasonalPayRunBatchStore.NewMemorySeasonalPayRunBatchStore();
        var item = SeasonalPayRunBatch.NewSeasonalPayRunBatch("code-669", "label-235");
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
        MemorySeasonalPayRunBatchStoreFindsWhatItSaved();
    }
}
