namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Functions of tiered certification checkpoint from adjustment.</summary>
public static class TieredCertificationCheckpointFromAdjustment
{
    public static Dictionary<string, double> SummarizeCertificationAdjustmentsForCheckpoint(List<CertificationAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<TieredCertificationCheckpoint> DeriveTieredCertificationCheckpointsFromAdjustment(List<CertificationAdjustment> items)
    {
        var outValue = new List<TieredCertificationCheckpoint>();
        foreach (var item in items)
        {
            var derived = TieredCertificationCheckpoint.NewTieredCertificationCheckpoint(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateTieredCertificationCheckpoint(TieredCertificationCheckpoint item)
    {
        return CertificationEstimatePolicy.ClassifyCertificationEstimate(item.WeightedTotal());
    }
}
