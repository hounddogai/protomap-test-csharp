namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.Certification.CertificationOverride;

/// <summary>Functions of tiered training due audit from override.</summary>
public static class TieredTrainingDueAuditFromOverride
{
    public static Dictionary<string, double> SummarizeDefaultCertificationOverridesForAudit(List<DefaultCertificationOverride> items)
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

    public static List<TieredTrainingDueAudit> DeriveTieredTrainingDueAuditsFromOverride(List<DefaultCertificationOverride> items)
    {
        var outValue = new List<TieredTrainingDueAudit>();
        foreach (var item in items)
        {
            var derived = TieredTrainingDueAudit.NewTieredTrainingDueAudit(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitTieredTrainingDueAudit(TieredTrainingDueAudit item)
    {
        return CertificationTierPolicy.CertificationTierBucket(item.NetValue());
    }
}
