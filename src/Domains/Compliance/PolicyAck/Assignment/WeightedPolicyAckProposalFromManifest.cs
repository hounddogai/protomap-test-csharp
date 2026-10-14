namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Functions of weighted policy ack proposal from manifest.</summary>
public static class WeightedPolicyAckProposalFromManifest
{
    public static Dictionary<string, double> SummarizeStrictAuditManifestsForProposal(List<StrictAuditManifest> items)
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

    public static List<WeightedPolicyAckProposal> DeriveWeightedPolicyAckProposalsFromManifest(List<StrictAuditManifest> items)
    {
        var outValue = new List<WeightedPolicyAckProposal>();
        foreach (var item in items)
        {
            var derived = WeightedPolicyAckProposal.NewWeightedPolicyAckProposal(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeWeightedPolicyAckProposal(WeightedPolicyAckProposal item)
    {
        return ManualAuditSchedulePolicy.ManualAuditScheduleBucket(item.EffectiveExposure());
    }
}
