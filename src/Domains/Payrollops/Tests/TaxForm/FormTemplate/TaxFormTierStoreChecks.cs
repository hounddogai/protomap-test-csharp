namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormTemplate;

using Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>Checks of form template code, run by the project's runner.</summary>
public static class TaxFormTierStoreChecks
{
    public static void MemoryTaxFormTierStoreFindsWhatItSaved()
    {
        var store = TaxFormTierStore.NewMemoryTaxFormTierStore();
        var item = TaxFormTier.NewTaxFormTier("code-187", "name-469");
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
        MemoryTaxFormTierStoreFindsWhatItSaved();
    }
}
