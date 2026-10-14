namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormMetric;

using Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>Checks of form metric code, run by the project's runner.</summary>
public static class TaxFormClaimStoreChecks
{
    public static void MemoryTaxFormClaimStoreFindsWhatItSaved()
    {
        var store = TaxFormClaimStore.NewMemoryTaxFormClaimStore();
        var item = TaxFormClaim.NewTaxFormClaim("code-460", "name-312");
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
        MemoryTaxFormClaimStoreFindsWhatItSaved();
    }
}
