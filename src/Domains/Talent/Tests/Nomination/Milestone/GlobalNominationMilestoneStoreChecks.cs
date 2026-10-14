namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class GlobalNominationMilestoneStoreChecks
{
    public static void MemoryGlobalNominationMilestoneStoreFindsWhatItSaved()
    {
        var store = GlobalNominationMilestoneStore.NewMemoryGlobalNominationMilestoneStore();
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("code-629", "label-807");
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
        MemoryGlobalNominationMilestoneStoreFindsWhatItSaved();
    }
}
