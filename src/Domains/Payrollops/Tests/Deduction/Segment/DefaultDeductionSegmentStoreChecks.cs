namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultDeductionSegmentStoreChecks
{
    public static void MemoryDefaultDeductionSegmentStoreFindsWhatItSaved()
    {
        var store = DefaultDeductionSegmentStore.NewMemoryDefaultDeductionSegmentStore();
        var item = DefaultDeductionSegment.NewDefaultDeductionSegment("code-175", "label-175");
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
        MemoryDefaultDeductionSegmentStoreFindsWhatItSaved();
    }
}
