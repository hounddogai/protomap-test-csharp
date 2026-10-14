namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class PendingTaxFormOverrideStoreChecks
{
    public static void MemoryPendingTaxFormOverrideStoreFindsWhatItSaved()
    {
        var store = PendingTaxFormOverrideStore.NewMemoryPendingTaxFormOverrideStore();
        var item = PendingTaxFormOverride.NewPendingTaxFormOverride("code-423", "title-560");
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
        MemoryPendingTaxFormOverrideStoreFindsWhatItSaved();
    }
}
