namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Functions of lenient policy ack milestone from profile.</summary>
public static class LenientPolicyAckMilestoneFromProfile
{
    public static Dictionary<string, double> SummarizeWeightedTrainingDueProfilesForMilestone(List<WeightedTrainingDueProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<LenientPolicyAckMilestone> DeriveLenientPolicyAckMilestonesFromProfile(List<WeightedTrainingDueProfile> items)
    {
        var outValue = new List<LenientPolicyAckMilestone>();
        foreach (var item in items)
        {
            var derived = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignLenientPolicyAckMilestone(LenientPolicyAckMilestone item)
    {
        return TrainingDueQuotaPolicy.TrainingDueQuotaBucket(item.AdjustedVolume());
    }
}
