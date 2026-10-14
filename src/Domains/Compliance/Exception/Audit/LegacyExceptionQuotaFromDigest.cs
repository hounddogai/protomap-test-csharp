namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Compliance.Certification.Manifest;

/// <summary>Functions of legacy exception quota from digest.</summary>
public static class LegacyExceptionQuotaFromDigest
{
    public static Dictionary<string, double> SummarizeCertificationDigestsForQuota(List<CertificationDigest> items)
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

    public static List<LegacyExceptionQuota> DeriveLegacyExceptionQuotasFromDigest(List<CertificationDigest> items)
    {
        var outValue = new List<LegacyExceptionQuota>();
        foreach (var item in items)
        {
            var derived = LegacyExceptionQuota.NewLegacyExceptionQuota(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankLegacyExceptionQuota(LegacyExceptionQuota item)
    {
        return CertificationManifestPolicy.ClassifyCertificationManifest(item.AdjustedVolume());
    }
}
