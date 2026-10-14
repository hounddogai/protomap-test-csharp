namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Functions of manual policy ack group from reservation.</summary>
public static class ManualPolicyAckGroupFromReservation
{
    public static Dictionary<string, double> SummarizeAuditReservationsForGroup(List<AuditReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<ManualPolicyAckGroup> DeriveManualPolicyAckGroupsFromReservation(List<AuditReservation> items)
    {
        var outValue = new List<ManualPolicyAckGroup>();
        foreach (var item in items)
        {
            var derived = ManualPolicyAckGroup.NewManualPolicyAckGroup(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreManualPolicyAckGroup(ManualPolicyAckGroup item)
    {
        return AuditQuotaPolicy.NormalizeAuditQuotaCode(item.Code);
    }
}
