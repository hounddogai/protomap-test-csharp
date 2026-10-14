namespace Acme.Workforce.Compliance.PolicyAck.Quota;

using Acme.Workforce.Compliance.Exception.Plan;

/// <summary>Functions of policy ack record from plan.</summary>
public static class PolicyAckRecordFromPlan
{
    public static Dictionary<string, double> SummarizePendingExceptionPlansForRecord(List<PendingExceptionPlan> items)
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

    public static List<PolicyAckRecord> DerivePolicyAckRecordsFromPlan(List<PendingExceptionPlan> items)
    {
        var outValue = new List<PolicyAckRecord>();
        foreach (var item in items)
        {
            var derived = PolicyAckRecord.NewPolicyAckRecord(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankPolicyAckRecord(PolicyAckRecord item)
    {
        return PrimaryExceptionRequestPolicy.PrimaryExceptionRequestBucket(item.ProjectedValue());
    }
}
