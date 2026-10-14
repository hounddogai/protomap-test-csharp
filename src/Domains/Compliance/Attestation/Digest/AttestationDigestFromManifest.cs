namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Functions of attestation digest from manifest.</summary>
public static class AttestationDigestFromManifest
{
    public static Dictionary<string, double> SummarizeSeasonalExceptionManifestsForDigest(List<SeasonalExceptionManifest> items)
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

    public static List<AttestationDigest> DeriveAttestationDigestsFromManifest(List<SeasonalExceptionManifest> items)
    {
        var outValue = new List<AttestationDigest>();
        foreach (var item in items)
        {
            var derived = AttestationDigest.NewAttestationDigest(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitAttestationDigest(AttestationDigest item)
    {
        return SeasonalExceptionManifestPolicy.SeasonalExceptionManifestBucket(item.WeightedVolume());
    }
}
