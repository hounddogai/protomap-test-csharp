namespace Acme.Workforce.Payrollops.Tests.TaxForm.Rollup;

using Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TaxFormBaselineStoreChecks
{
    public static void MemoryTaxFormBaselineStoreFindsWhatItSaved()
    {
        var store = TaxFormBaselineStore.NewMemoryTaxFormBaselineStore();
        var item = TaxFormBaseline.NewTaxFormBaseline("code-209", "title-901");
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
        MemoryTaxFormBaselineStoreFindsWhatItSaved();
    }
}
