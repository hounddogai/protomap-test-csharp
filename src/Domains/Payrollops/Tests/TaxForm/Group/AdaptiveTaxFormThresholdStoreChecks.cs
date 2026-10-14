namespace Acme.Workforce.Payrollops.Tests.TaxForm.Group;

using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class AdaptiveTaxFormThresholdStoreChecks
{
    public static void MemoryAdaptiveTaxFormThresholdStoreFindsWhatItSaved()
    {
        var store = AdaptiveTaxFormThresholdStore.NewMemoryAdaptiveTaxFormThresholdStore();
        var item = AdaptiveTaxFormThreshold.NewAdaptiveTaxFormThreshold("code-426", "name-162");
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
        MemoryAdaptiveTaxFormThresholdStoreFindsWhatItSaved();
    }
}
