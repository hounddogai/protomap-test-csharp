namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Compliance.Exception.Cycle;

/// <summary>Functions of scheduled policy ack quota from cycle.</summary>
public static class ScheduledPolicyAckQuotaFromCycle
{
    public static Dictionary<string, double> SummarizeExceptionCyclesForQuota(List<ExceptionCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<ScheduledPolicyAckQuota> DeriveScheduledPolicyAckQuotasFromCycle(List<ExceptionCycle> items)
    {
        var outValue = new List<ScheduledPolicyAckQuota>();
        foreach (var item in items)
        {
            var derived = ScheduledPolicyAckQuota.NewScheduledPolicyAckQuota(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignScheduledPolicyAckQuota(ScheduledPolicyAckQuota item)
    {
        return ExceptionCyclePolicy.NormalizeExceptionCycleCode(item.Code);
    }
}
