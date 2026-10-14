namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class SeasonalTaxFormTierStoreChecks
{
    public static void MemorySeasonalTaxFormTierStoreFindsWhatItSaved()
    {
        var store = SeasonalTaxFormTierStore.NewMemorySeasonalTaxFormTierStore();
        var item = SeasonalTaxFormTier.NewSeasonalTaxFormTier("code-126", "label-582");
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
        MemorySeasonalTaxFormTierStoreFindsWhatItSaved();
    }
}
