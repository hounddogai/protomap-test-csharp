namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>Functions of policy ack assignment from baseline.</summary>
public static class PolicyAckAssignmentFromBaseline
{
    public static Dictionary<string, double> SummarizeFallbackTrainingDueBaselinesForAssignment(List<FallbackTrainingDueBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<PolicyAckAssignment> DerivePolicyAckAssignmentsFromBaseline(List<FallbackTrainingDueBaseline> items)
    {
        var outValue = new List<PolicyAckAssignment>();
        foreach (var item in items)
        {
            var derived = PolicyAckAssignment.NewPolicyAckAssignment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPolicyAckAssignment(PolicyAckAssignment item)
    {
        return CappedTrainingDueDigestPolicy.CappedTrainingDueDigestBucket(item.AdjustedValue());
    }
}
