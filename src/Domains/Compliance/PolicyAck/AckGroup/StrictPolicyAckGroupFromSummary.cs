namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

using Acme.Workforce.Compliance.Attestation.Quota;

/// <summary>Functions of strict policy ack group from summary.</summary>
public static class StrictPolicyAckGroupFromSummary
{
    public static Dictionary<string, double> SummarizeAttestationSummariesForGroup(List<AttestationSummary> items)
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

    public static List<StrictPolicyAckGroup> DeriveStrictPolicyAckGroupsFromSummary(List<AttestationSummary> items)
    {
        var outValue = new List<StrictPolicyAckGroup>();
        foreach (var item in items)
        {
            var derived = StrictPolicyAckGroup.NewStrictPolicyAckGroup(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveStrictPolicyAckGroup(StrictPolicyAckGroup item)
    {
        return AttestationBudgetPolicy.NormalizeAttestationBudgetCode(item.Code);
    }
}
