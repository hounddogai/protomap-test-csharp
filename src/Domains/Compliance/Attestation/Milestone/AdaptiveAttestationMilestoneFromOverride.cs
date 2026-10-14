namespace Acme.Workforce.Compliance.Attestation.Milestone;

using Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>Functions of adaptive attestation milestone from override.</summary>
public static class AdaptiveAttestationMilestoneFromOverride
{
    public static Dictionary<string, double> SummarizeCertificationOverridesForMilestone(List<CertificationOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<AdaptiveAttestationMilestone> DeriveAdaptiveAttestationMilestonesFromOverride(List<CertificationOverride> items)
    {
        var outValue = new List<AdaptiveAttestationMilestone>();
        foreach (var item in items)
        {
            var derived = AdaptiveAttestationMilestone.NewAdaptiveAttestationMilestone(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateAdaptiveAttestationMilestone(AdaptiveAttestationMilestone item)
    {
        return CertificationAuditPolicy.CertificationAuditBucket(item.ProjectedLoad());
    }
}
