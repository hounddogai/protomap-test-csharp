namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedRateMilestoneStoreChecks
{
    public static void MemoryCappedRateMilestoneStoreFindsWhatItSaved()
    {
        var store = CappedRateMilestoneStore.NewMemoryCappedRateMilestoneStore();
        var item = CappedRateMilestone.NewCappedRateMilestone("code-428", "label-654");
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
        MemoryCappedRateMilestoneStoreFindsWhatItSaved();
    }
}
