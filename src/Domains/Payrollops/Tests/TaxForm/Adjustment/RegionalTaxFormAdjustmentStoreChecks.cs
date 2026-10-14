namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RegionalTaxFormAdjustmentStoreChecks
{
    public static void MemoryRegionalTaxFormAdjustmentStoreFindsWhatItSaved()
    {
        var store = RegionalTaxFormAdjustmentStore.NewMemoryRegionalTaxFormAdjustmentStore();
        var item = RegionalTaxFormAdjustment.NewRegionalTaxFormAdjustment("code-137", "title-83");
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
        MemoryRegionalTaxFormAdjustmentStoreFindsWhatItSaved();
    }
}
