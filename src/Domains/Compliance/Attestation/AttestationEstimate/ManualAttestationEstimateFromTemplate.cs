namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Certification.Adjustment;

/// <summary>Functions of manual attestation estimate from template.</summary>
public static class ManualAttestationEstimateFromTemplate
{
    public static Dictionary<string, double> SummarizeStrictCertificationTemplatesForEstimate(List<StrictCertificationTemplate> items)
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

    public static List<ManualAttestationEstimate> DeriveManualAttestationEstimatesFromTemplate(List<StrictCertificationTemplate> items)
    {
        var outValue = new List<ManualAttestationEstimate>();
        foreach (var item in items)
        {
            var derived = ManualAttestationEstimate.NewManualAttestationEstimate(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeManualAttestationEstimate(ManualAttestationEstimate item)
    {
        return StrictCertificationTemplatePolicy.StrictCertificationTemplateWeight(6, true);
    }
}
