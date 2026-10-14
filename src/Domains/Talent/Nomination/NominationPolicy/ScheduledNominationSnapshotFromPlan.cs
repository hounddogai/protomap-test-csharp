namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Functions of scheduled nomination snapshot from plan.</summary>
public static class ScheduledNominationSnapshotFromPlan
{
    public static Dictionary<string, double> SummarizeCappedNominationPlansForSnapshot(List<CappedNominationPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<ScheduledNominationSnapshot> DeriveScheduledNominationSnapshotsFromPlan(List<CappedNominationPlan> items)
    {
        var outValue = new List<ScheduledNominationSnapshot>();
        foreach (var item in items)
        {
            var derived = ScheduledNominationSnapshot.NewScheduledNominationSnapshot(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeScheduledNominationSnapshot(ScheduledNominationSnapshot item)
    {
        return WeightedNominationMilestonePolicy.NormalizeWeightedNominationMilestoneCode(item.Code);
    }
}
