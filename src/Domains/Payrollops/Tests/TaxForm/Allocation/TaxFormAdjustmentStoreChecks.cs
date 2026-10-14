namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAdjustmentStoreChecks
{
    public static void MemoryTaxFormAdjustmentStoreFindsWhatItSaved()
    {
        var store = TaxFormAdjustmentStore.NewMemoryTaxFormAdjustmentStore();
        var item = TaxFormAdjustment.NewTaxFormAdjustment("code-803", "label-588");
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
        MemoryTaxFormAdjustmentStoreFindsWhatItSaved();
    }
}
