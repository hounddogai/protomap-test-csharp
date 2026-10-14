namespace Acme.Workforce.Assets.Tests.Assignment.Estimate;

using Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ScheduledAssignmentEstimateStoreChecks
{
    public static void MemoryScheduledAssignmentEstimateStoreFindsWhatItSaved()
    {
        var store = ScheduledAssignmentEstimateStore.NewMemoryScheduledAssignmentEstimateStore();
        var item = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate("code-625", "label-906");
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
        MemoryScheduledAssignmentEstimateStoreFindsWhatItSaved();
    }
}
