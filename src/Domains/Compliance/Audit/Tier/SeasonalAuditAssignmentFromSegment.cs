namespace Acme.Workforce.Compliance.Audit.Tier;

using Acme.Workforce.Compliance.Attestation.Rollup;

/// <summary>Functions of seasonal audit assignment from segment.</summary>
public static class SeasonalAuditAssignmentFromSegment
{
    public static Dictionary<string, double> SummarizeAttestationSegmentsForAssignment(List<AttestationSegment> items)
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

    public static List<SeasonalAuditAssignment> DeriveSeasonalAuditAssignmentsFromSegment(List<AttestationSegment> items)
    {
        var outValue = new List<SeasonalAuditAssignment>();
        foreach (var item in items)
        {
            var derived = SeasonalAuditAssignment.NewSeasonalAuditAssignment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeSeasonalAuditAssignment(SeasonalAuditAssignment item)
    {
        return AttestationCyclePolicy.NormalizeAttestationCycleCode(item.Code);
    }
}
