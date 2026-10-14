namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ReturnPlanSegmentStoreChecks
{
    public static void MemoryReturnPlanSegmentStoreFindsWhatItSaved()
    {
        var store = ReturnPlanSegmentStore.NewMemoryReturnPlanSegmentStore();
        var item = ReturnPlanSegment.NewReturnPlanSegment("code-965", "name-620");
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
        MemoryReturnPlanSegmentStoreFindsWhatItSaved();
    }
}
