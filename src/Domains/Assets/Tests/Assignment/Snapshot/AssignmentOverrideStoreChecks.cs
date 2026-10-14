namespace Acme.Workforce.Assets.Tests.Assignment.Snapshot;

using Acme.Workforce.Assets.Assignment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class AssignmentOverrideStoreChecks
{
    public static void MemoryAssignmentOverrideStoreFindsWhatItSaved()
    {
        var store = AssignmentOverrideStore.NewMemoryAssignmentOverrideStore();
        var item = AssignmentOverride.NewAssignmentOverride("code-229", "label-505");
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
        MemoryAssignmentOverrideStoreFindsWhatItSaved();
    }
}
