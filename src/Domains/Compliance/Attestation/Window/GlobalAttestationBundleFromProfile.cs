namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Compliance.Certification.CertificationOverride;

/// <summary>Functions of global attestation bundle from profile.</summary>
public static class GlobalAttestationBundleFromProfile
{
    public static Dictionary<string, double> SummarizeCertificationProfilesForBundle(List<CertificationProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<GlobalAttestationBundle> DeriveGlobalAttestationBundlesFromProfile(List<CertificationProfile> items)
    {
        var outValue = new List<GlobalAttestationBundle>();
        foreach (var item in items)
        {
            var derived = GlobalAttestationBundle.NewGlobalAttestationBundle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeGlobalAttestationBundle(GlobalAttestationBundle item)
    {
        return DefaultCertificationOverridePolicy.NormalizeDefaultCertificationOverrideCode(item.Code);
    }
}
