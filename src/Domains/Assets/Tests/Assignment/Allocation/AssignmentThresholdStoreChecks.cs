namespace Acme.Workforce.Assets.Tests.Assignment.Allocation;

using Acme.Workforce.Assets.Assignment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AssignmentThresholdStoreChecks
{
    public static void MemoryAssignmentThresholdStoreFindsWhatItSaved()
    {
        var store = AssignmentThresholdStore.NewMemoryAssignmentThresholdStore();
        var item = AssignmentThreshold.NewAssignmentThreshold("code-684", "label-90");
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
        MemoryAssignmentThresholdStoreFindsWhatItSaved();
    }
}
