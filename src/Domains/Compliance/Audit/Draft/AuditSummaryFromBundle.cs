namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Functions of audit summary from bundle.</summary>
public static class AuditSummaryFromBundle
{
    public static Dictionary<string, double> SummarizeGlobalAttestationBundlesForSummary(List<GlobalAttestationBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<AuditSummary> DeriveAuditSummariesFromBundle(List<GlobalAttestationBundle> items)
    {
        var outValue = new List<AuditSummary>();
        foreach (var item in items)
        {
            var derived = AuditSummary.NewAuditSummary(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveAuditSummary(AuditSummary item)
    {
        return AttestationNoticePolicy.ClassifyAttestationNotice(item.ProjectedTotal());
    }
}
