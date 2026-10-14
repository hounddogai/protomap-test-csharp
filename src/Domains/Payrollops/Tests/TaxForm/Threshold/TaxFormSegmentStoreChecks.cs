namespace Acme.Workforce.Payrollops.Tests.TaxForm.Threshold;

using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class TaxFormSegmentStoreChecks
{
    public static void MemoryTaxFormSegmentStoreFindsWhatItSaved()
    {
        var store = TaxFormSegmentStore.NewMemoryTaxFormSegmentStore();
        var item = TaxFormSegment.NewTaxFormSegment("code-115", "name-144");
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
        MemoryTaxFormSegmentStoreFindsWhatItSaved();
    }
}
