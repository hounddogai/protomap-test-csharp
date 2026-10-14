namespace Acme.Workforce.Compliance.PolicyAck.Notice;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Functions of policy ack rollup from schedule.</summary>
public static class PolicyAckRollupFromSchedule
{
    public static Dictionary<string, double> SummarizeRegionalAttestationSchedulesForRollup(List<RegionalAttestationSchedule> items)
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

    public static List<PolicyAckRollup> DerivePolicyAckRollupsFromSchedule(List<RegionalAttestationSchedule> items)
    {
        var outValue = new List<PolicyAckRollup>();
        foreach (var item in items)
        {
            var derived = PolicyAckRollup.NewPolicyAckRollup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyPolicyAckRollupExtra(PolicyAckRollup item)
    {
        return PrimaryAttestationEntryPolicy.ClassifyPrimaryAttestationEntry(item.ProjectedExposure());
    }
}
