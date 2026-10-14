namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentClaim;

using Acme.Workforce.Assets.Assignment.AssignmentClaim;

/// <summary>Checks of assignment claim code, run by the project's runner.</summary>
public static class SeasonalAssignmentAllocationStoreChecks
{
    public static void MemorySeasonalAssignmentAllocationStoreFindsWhatItSaved()
    {
        var store = SeasonalAssignmentAllocationStore.NewMemorySeasonalAssignmentAllocationStore();
        var item = SeasonalAssignmentAllocation.NewSeasonalAssignmentAllocation("code-641", "name-40");
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
        MemorySeasonalAssignmentAllocationStoreFindsWhatItSaved();
    }
}
