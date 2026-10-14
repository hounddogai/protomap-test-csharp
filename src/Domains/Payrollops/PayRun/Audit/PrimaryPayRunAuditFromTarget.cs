namespace Acme.Workforce.Payrollops.PayRun.Audit;

using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Functions of primary pay run audit from target.</summary>
public static class PrimaryPayRunAuditFromTarget
{
    public static Dictionary<string, double> SummarizeGarnishmentTargetsForAudit(List<GarnishmentTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<PrimaryPayRunAudit> DerivePrimaryPayRunAuditsFromTarget(List<GarnishmentTarget> items)
    {
        var outValue = new List<PrimaryPayRunAudit>();
        foreach (var item in items)
        {
            var derived = PrimaryPayRunAudit.NewPrimaryPayRunAudit(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizePrimaryPayRunAudit(PrimaryPayRunAudit item)
    {
        return GarnishmentTargetPolicy.ClassifyGarnishmentTarget(item.ProjectedValue());
    }
}
