namespace Acme.Workforce.Payrollops.Tests.TaxForm.Exception;

using Acme.Workforce.Payrollops.TaxForm.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LegacyTaxFormExceptionStoreChecks
{
    public static void MemoryLegacyTaxFormExceptionStoreFindsWhatItSaved()
    {
        var store = LegacyTaxFormExceptionStore.NewMemoryLegacyTaxFormExceptionStore();
        var item = LegacyTaxFormException.NewLegacyTaxFormException("code-775", "title-720");
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
        MemoryLegacyTaxFormExceptionStoreFindsWhatItSaved();
    }
}
