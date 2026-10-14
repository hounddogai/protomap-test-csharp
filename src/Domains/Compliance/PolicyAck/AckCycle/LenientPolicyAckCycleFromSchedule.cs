namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Functions of lenient policy ack cycle from schedule.</summary>
public static class LenientPolicyAckCycleFromSchedule
{
    public static Dictionary<string, double> SummarizeAuditSchedulesForCycle(List<AuditSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<LenientPolicyAckCycle> DeriveLenientPolicyAckCyclesFromSchedule(List<AuditSchedule> items)
    {
        var outValue = new List<LenientPolicyAckCycle>();
        foreach (var item in items)
        {
            var derived = LenientPolicyAckCycle.NewLenientPolicyAckCycle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyLenientPolicyAckCycle(LenientPolicyAckCycle item)
    {
        return AuditBatchPolicy.ClassifyAuditBatch(item.ProjectedVolume());
    }
}
