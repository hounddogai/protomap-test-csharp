namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TaxFormBatchStoreChecks
{
    public static void MemoryTaxFormBatchStoreFindsWhatItSaved()
    {
        var store = TaxFormBatchStore.NewMemoryTaxFormBatchStore();
        var item = TaxFormBatch.NewTaxFormBatch("code-897", "label-465");
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
        MemoryTaxFormBatchStoreFindsWhatItSaved();
    }
}
