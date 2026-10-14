namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class SeasonalAdjustmentAssignmentStoreChecks
{
    public static void MemorySeasonalAdjustmentAssignmentStoreFindsWhatItSaved()
    {
        var store = SeasonalAdjustmentAssignmentStore.NewMemorySeasonalAdjustmentAssignmentStore();
        var item = SeasonalAdjustmentAssignment.NewSeasonalAdjustmentAssignment("code-66", "title-876");
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
        MemorySeasonalAdjustmentAssignmentStoreFindsWhatItSaved();
    }
}
