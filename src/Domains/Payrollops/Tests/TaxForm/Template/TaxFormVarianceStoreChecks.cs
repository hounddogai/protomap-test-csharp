namespace Acme.Workforce.Payrollops.Tests.TaxForm.Template;

using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TaxFormVarianceStoreChecks
{
    public static void MemoryTaxFormVarianceStoreFindsWhatItSaved()
    {
        var store = TaxFormVarianceStore.NewMemoryTaxFormVarianceStore();
        var item = TaxFormVariance.NewTaxFormVariance("code-21", "label-217");
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
        MemoryTaxFormVarianceStoreFindsWhatItSaved();
    }
}
