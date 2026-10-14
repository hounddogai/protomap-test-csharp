namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictDeductionSegmentStoreChecks
{
    public static void MemoryStrictDeductionSegmentStoreFindsWhatItSaved()
    {
        var store = StrictDeductionSegmentStore.NewMemoryStrictDeductionSegmentStore();
        var item = StrictDeductionSegment.NewStrictDeductionSegment("code-200", "name-78");
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
        MemoryStrictDeductionSegmentStoreFindsWhatItSaved();
    }
}
