namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Compliance.TrainingDue.Draft;

/// <summary>Functions of tiered policy ack entry from assignment.</summary>
public static class TieredPolicyAckEntryFromAssignment
{
    public static Dictionary<string, double> SummarizeWeightedTrainingDueAssignmentsForEntry(List<WeightedTrainingDueAssignment> items)
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

    public static List<TieredPolicyAckEntry> DeriveTieredPolicyAckEntriesFromAssignment(List<WeightedTrainingDueAssignment> items)
    {
        var outValue = new List<TieredPolicyAckEntry>();
        foreach (var item in items)
        {
            var derived = TieredPolicyAckEntry.NewTieredPolicyAckEntry(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchTieredPolicyAckEntry(TieredPolicyAckEntry item)
    {
        return WeightedTrainingDueAssignmentPolicy.WeightedTrainingDueAssignmentBucket(item.AdjustedValue());
    }
}
