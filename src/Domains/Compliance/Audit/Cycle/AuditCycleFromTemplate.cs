namespace Acme.Workforce.Compliance.Audit.Cycle;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Functions of audit cycle from template.</summary>
public static class AuditCycleFromTemplate
{
    public static Dictionary<string, double> SummarizeExceptionTemplatesForCycle(List<ExceptionTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<AuditCycle> DeriveAuditCyclesFromTemplate(List<ExceptionTemplate> items)
    {
        var outValue = new List<AuditCycle>();
        foreach (var item in items)
        {
            var derived = AuditCycle.NewAuditCycle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateAuditCycle(AuditCycle item)
    {
        return LenientExceptionVariancePolicy.LenientExceptionVarianceBucket(item.AdjustedTotal());
    }
}
