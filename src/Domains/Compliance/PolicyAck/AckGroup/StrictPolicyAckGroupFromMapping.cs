namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

using Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>Functions of strict policy ack group from mapping.</summary>
public static class StrictPolicyAckGroupFromMapping
{
    public static Dictionary<string, double> SummarizePrimaryAuditMappingsForGroup(List<PrimaryAuditMapping> items)
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

    public static List<StrictPolicyAckGroup> DeriveStrictPolicyAckGroupsFromMapping(List<PrimaryAuditMapping> items)
    {
        var outValue = new List<StrictPolicyAckGroup>();
        foreach (var item in items)
        {
            var derived = StrictPolicyAckGroup.NewStrictPolicyAckGroup(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignStrictPolicyAckGroup(StrictPolicyAckGroup item)
    {
        return CappedAuditReservationPolicy.CappedAuditReservationBucket(item.AdjustedExposure());
    }
}
