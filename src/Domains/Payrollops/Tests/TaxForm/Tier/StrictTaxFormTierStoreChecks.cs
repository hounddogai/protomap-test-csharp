namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictTaxFormTierStoreChecks
{
    public static void MemoryStrictTaxFormTierStoreFindsWhatItSaved()
    {
        var store = StrictTaxFormTierStore.NewMemoryStrictTaxFormTierStore();
        var item = StrictTaxFormTier.NewStrictTaxFormTier("code-913", "label-939");
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
        MemoryStrictTaxFormTierStoreFindsWhatItSaved();
    }
}
