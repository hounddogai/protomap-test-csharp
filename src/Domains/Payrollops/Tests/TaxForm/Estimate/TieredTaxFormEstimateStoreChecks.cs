namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredTaxFormEstimateStoreChecks
{
    public static void MemoryTieredTaxFormEstimateStoreFindsWhatItSaved()
    {
        var store = TieredTaxFormEstimateStore.NewMemoryTieredTaxFormEstimateStore();
        var item = TieredTaxFormEstimate.NewTieredTaxFormEstimate("code-415", "name-118");
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
        MemoryTieredTaxFormEstimateStoreFindsWhatItSaved();
    }
}
