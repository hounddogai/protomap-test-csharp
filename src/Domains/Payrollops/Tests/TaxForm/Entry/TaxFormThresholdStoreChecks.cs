namespace Acme.Workforce.Payrollops.Tests.TaxForm.Entry;

using Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TaxFormThresholdStoreChecks
{
    public static void MemoryTaxFormThresholdStoreFindsWhatItSaved()
    {
        var store = TaxFormThresholdStore.NewMemoryTaxFormThresholdStore();
        var item = TaxFormThreshold.NewTaxFormThreshold("code-378", "label-67");
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
        MemoryTaxFormThresholdStoreFindsWhatItSaved();
    }
}
