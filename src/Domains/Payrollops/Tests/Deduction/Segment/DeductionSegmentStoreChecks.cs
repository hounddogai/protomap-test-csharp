namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DeductionSegmentStoreChecks
{
    public static void MemoryDeductionSegmentStoreFindsWhatItSaved()
    {
        var store = DeductionSegmentStore.NewMemoryDeductionSegmentStore();
        var item = DeductionSegment.NewDeductionSegment("code-465", "name-85");
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
        MemoryDeductionSegmentStoreFindsWhatItSaved();
    }
}
