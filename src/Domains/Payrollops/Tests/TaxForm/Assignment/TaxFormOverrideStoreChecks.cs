namespace Acme.Workforce.Payrollops.Tests.TaxForm.Assignment;

using Acme.Workforce.Payrollops.TaxForm.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TaxFormOverrideStoreChecks
{
    public static void MemoryTaxFormOverrideStoreFindsWhatItSaved()
    {
        var store = TaxFormOverrideStore.NewMemoryTaxFormOverrideStore();
        var item = TaxFormOverride.NewTaxFormOverride("code-86", "title-537");
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
        MemoryTaxFormOverrideStoreFindsWhatItSaved();
    }
}
