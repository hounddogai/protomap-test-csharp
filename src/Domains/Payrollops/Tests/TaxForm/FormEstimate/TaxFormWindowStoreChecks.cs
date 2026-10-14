namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class TaxFormWindowStoreChecks
{
    public static void MemoryTaxFormWindowStoreFindsWhatItSaved()
    {
        var store = TaxFormWindowStore.NewMemoryTaxFormWindowStore();
        var item = TaxFormWindow.NewTaxFormWindow("code-250", "name-515");
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
        MemoryTaxFormWindowStoreFindsWhatItSaved();
    }
}
