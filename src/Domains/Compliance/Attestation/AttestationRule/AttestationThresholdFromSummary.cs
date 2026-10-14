namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Certification.CertificationSummary;

/// <summary>Functions of attestation threshold from summary.</summary>
public static class AttestationThresholdFromSummary
{
    public static Dictionary<string, double> SummarizeRollingCertificationSummariesForThreshold(List<RollingCertificationSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<AttestationThreshold> DeriveAttestationThresholdsFromSummary(List<RollingCertificationSummary> items)
    {
        var outValue = new List<AttestationThreshold>();
        foreach (var item in items)
        {
            var derived = AttestationThreshold.NewAttestationThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeAttestationThreshold(AttestationThreshold item)
    {
        return RollingCertificationSummaryPolicy.ClassifyRollingCertificationSummary(item.ProjectedValue());
    }
}
