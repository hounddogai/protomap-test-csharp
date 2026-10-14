namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentAssignmentStoreChecks
{
    public static void MemoryAdjustmentAssignmentStoreFindsWhatItSaved()
    {
        var store = AdjustmentAssignmentStore.NewMemoryAdjustmentAssignmentStore();
        var item = AdjustmentAssignment.NewAdjustmentAssignment("code-431", "name-979");
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
        MemoryAdjustmentAssignmentStoreFindsWhatItSaved();
    }
}
