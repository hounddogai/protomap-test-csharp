namespace Acme.Workforce.Engagement.ActionPlan.Snapshot;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Functions of action plan snapshot from digest.</summary>
public static class ActionPlanSnapshotFromDigest
{
    public static Dictionary<string, double> SummarizeLegacyActionPlanDigestsForSnapshot(List<LegacyActionPlanDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<ActionPlanSnapshot> DeriveActionPlanSnapshotsFromDigest(List<LegacyActionPlanDigest> items)
    {
        var outValue = new List<ActionPlanSnapshot>();
        foreach (var item in items)
        {
            var derived = ActionPlanSnapshot.NewActionPlanSnapshot(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitActionPlanSnapshot(ActionPlanSnapshot item)
    {
        return BatchedActionPlanBundlePolicy.NormalizeBatchedActionPlanBundleCode(item.Code);
    }
}
