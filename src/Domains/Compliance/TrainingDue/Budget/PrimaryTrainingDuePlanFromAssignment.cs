namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Compliance.PolicyAck.Assignment;

/// <summary>Functions of primary training due plan from assignment.</summary>
public static class PrimaryTrainingDuePlanFromAssignment
{
    public static Dictionary<string, double> SummarizePolicyAckAssignmentsForPlan(List<PolicyAckAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<PrimaryTrainingDuePlan> DerivePrimaryTrainingDuePlansFromAssignment(List<PolicyAckAssignment> items)
    {
        var outValue = new List<PrimaryTrainingDuePlan>();
        foreach (var item in items)
        {
            var derived = PrimaryTrainingDuePlan.NewPrimaryTrainingDuePlan(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergePrimaryTrainingDuePlan(PrimaryTrainingDuePlan item)
    {
        return WeightedPolicyAckProposalPolicy.ClassifyWeightedPolicyAckProposal(item.WeightedVolume());
    }
}
