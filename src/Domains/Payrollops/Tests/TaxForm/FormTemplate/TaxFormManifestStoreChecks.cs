namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormTemplate;

using Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>Checks of form template code, run by the project's runner.</summary>
public static class TaxFormManifestStoreChecks
{
    public static void MemoryTaxFormManifestStoreFindsWhatItSaved()
    {
        var store = TaxFormManifestStore.NewMemoryTaxFormManifestStore();
        var item = TaxFormManifest.NewTaxFormManifest("code-314", "title-785");
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
        MemoryTaxFormManifestStoreFindsWhatItSaved();
    }
}
