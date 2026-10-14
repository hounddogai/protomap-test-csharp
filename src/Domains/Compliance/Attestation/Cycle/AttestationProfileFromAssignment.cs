namespace Acme.Workforce.Compliance.Attestation.Cycle;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>Functions of attestation profile from assignment.</summary>
public static class AttestationProfileFromAssignment
{
    public static Dictionary<string, double> SummarizeCappedCertificationAssignmentsForProfile(List<CappedCertificationAssignment> items)
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

    public static List<AttestationProfile> DeriveAttestationProfilesFromAssignment(List<CappedCertificationAssignment> items)
    {
        var outValue = new List<AttestationProfile>();
        foreach (var item in items)
        {
            var derived = AttestationProfile.NewAttestationProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeAttestationProfile(AttestationProfile item)
    {
        return TieredCertificationSummaryPolicy.ClassifyTieredCertificationSummary(item.MeasuredValue());
    }
}
