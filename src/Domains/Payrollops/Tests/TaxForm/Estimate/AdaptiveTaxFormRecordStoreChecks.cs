namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AdaptiveTaxFormRecordStoreChecks
{
    public static void MemoryAdaptiveTaxFormRecordStoreFindsWhatItSaved()
    {
        var store = AdaptiveTaxFormRecordStore.NewMemoryAdaptiveTaxFormRecordStore();
        var item = AdaptiveTaxFormRecord.NewAdaptiveTaxFormRecord("code-852", "name-527");
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
        MemoryAdaptiveTaxFormRecordStoreFindsWhatItSaved();
    }
}
