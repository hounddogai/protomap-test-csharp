namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class WeightedNominationMilestoneStoreChecks
{
    public static void MemoryWeightedNominationMilestoneStoreFindsWhatItSaved()
    {
        var store = WeightedNominationMilestoneStore.NewMemoryWeightedNominationMilestoneStore();
        var item = WeightedNominationMilestone.NewWeightedNominationMilestone("code-77", "name-873");
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
        MemoryWeightedNominationMilestoneStoreFindsWhatItSaved();
    }
}
