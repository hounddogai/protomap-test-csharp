namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

using Acme.Workforce.Compliance.Certification.Schedule;

/// <summary>Functions of pending attestation threshold from bundle.</summary>
public static class PendingAttestationThresholdFromBundle
{
    public static Dictionary<string, double> SummarizeFallbackCertificationBundlesForThreshold(List<FallbackCertificationBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<PendingAttestationThreshold> DerivePendingAttestationThresholdsFromBundle(List<FallbackCertificationBundle> items)
    {
        var outValue = new List<PendingAttestationThreshold>();
        foreach (var item in items)
        {
            var derived = PendingAttestationThreshold.NewPendingAttestationThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePendingAttestationThreshold(PendingAttestationThreshold item)
    {
        return RegionalCertificationAdjustmentPolicy.NormalizeRegionalCertificationAdjustmentCode(item.Code);
    }
}
