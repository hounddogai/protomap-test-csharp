namespace Acme.Workforce.Compliance.Certification.Notice;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Functions of weighted certification batch from segment.</summary>
public static class WeightedCertificationBatchFromSegment
{
    public static Dictionary<string, double> SummarizePendingAttestationSegmentsForBatch(List<PendingAttestationSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<WeightedCertificationBatch> DeriveWeightedCertificationBatchesFromSegment(List<PendingAttestationSegment> items)
    {
        var outValue = new List<WeightedCertificationBatch>();
        foreach (var item in items)
        {
            var derived = WeightedCertificationBatch.NewWeightedCertificationBatch(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleWeightedCertificationBatch(WeightedCertificationBatch item)
    {
        return AttestationLedgerPolicy.NormalizeAttestationLedgerCode(item.Code);
    }
}
