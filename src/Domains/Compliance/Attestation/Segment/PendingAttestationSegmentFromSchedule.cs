namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Functions of pending attestation segment from schedule.</summary>
public static class PendingAttestationSegmentFromSchedule
{
    public static Dictionary<string, double> SummarizeAuditSchedulesForSegment(List<AuditSchedule> items)
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

    public static List<PendingAttestationSegment> DerivePendingAttestationSegmentsFromSchedule(List<AuditSchedule> items)
    {
        var outValue = new List<PendingAttestationSegment>();
        foreach (var item in items)
        {
            var derived = PendingAttestationSegment.NewPendingAttestationSegment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimatePendingAttestationSegment(PendingAttestationSegment item)
    {
        return WeightedAuditRecordPolicy.NormalizeWeightedAuditRecordCode(item.Code);
    }
}
