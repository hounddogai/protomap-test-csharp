namespace Acme.Workforce.Compliance.Certification.CertificationOverride;

using Acme.Workforce.Compliance.Exception.Group;

/// <summary>Functions of default certification override from group.</summary>
public static class DefaultCertificationOverrideFromGroup
{
    public static Dictionary<string, double> SummarizeExceptionGroupsForOverride(List<ExceptionGroup> items)
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

    public static List<DefaultCertificationOverride> DeriveDefaultCertificationOverridesFromGroup(List<ExceptionGroup> items)
    {
        var outValue = new List<DefaultCertificationOverride>();
        foreach (var item in items)
        {
            var derived = DefaultCertificationOverride.NewDefaultCertificationOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectDefaultCertificationOverride(DefaultCertificationOverride item)
    {
        return ExceptionPlanPolicy.ClassifyExceptionPlan(item.ProjectedVolume());
    }
}
