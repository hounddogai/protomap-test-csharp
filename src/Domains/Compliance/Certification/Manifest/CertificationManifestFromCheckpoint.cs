namespace Acme.Workforce.Compliance.Certification.Manifest;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Functions of certification manifest from checkpoint.</summary>
public static class CertificationManifestFromCheckpoint
{
    public static Dictionary<string, double> SummarizeExceptionCheckpointsForManifest(List<ExceptionCheckpoint> items)
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

    public static List<CertificationManifest> DeriveCertificationManifestsFromCheckpoint(List<ExceptionCheckpoint> items)
    {
        var outValue = new List<CertificationManifest>();
        foreach (var item in items)
        {
            var derived = CertificationManifest.NewCertificationManifest(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveCertificationManifest(CertificationManifest item)
    {
        return GlobalExceptionSnapshotPolicy.GlobalExceptionSnapshotBucket(item.WeightedValue());
    }
}
