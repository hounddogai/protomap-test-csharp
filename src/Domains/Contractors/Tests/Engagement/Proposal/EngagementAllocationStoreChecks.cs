namespace Acme.Workforce.Contractors.Tests.Engagement.Proposal;

using Acme.Workforce.Contractors.Engagement.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class EngagementAllocationStoreChecks
{
    public static void MemoryEngagementAllocationStoreFindsWhatItSaved()
    {
        var store = EngagementAllocationStore.NewMemoryEngagementAllocationStore();
        var item = EngagementAllocation.NewEngagementAllocation("code-34", "label-551");
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
        MemoryEngagementAllocationStoreFindsWhatItSaved();
    }
}
