namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

using Acme.Workforce.Compliance.Audit.Tier;

/// <summary>Functions of exception variance from assignment.</summary>
public static class ExceptionVarianceFromAssignment
{
    public static Dictionary<string, double> SummarizeSeasonalAuditAssignmentsForVariance(List<SeasonalAuditAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<ExceptionVariance> DeriveExceptionVariancesFromAssignment(List<SeasonalAuditAssignment> items)
    {
        var outValue = new List<ExceptionVariance>();
        foreach (var item in items)
        {
            var derived = ExceptionVariance.NewExceptionVariance(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileExceptionVariance(ExceptionVariance item)
    {
        return AuditTierPolicy.ClassifyAuditTier(item.NetValue());
    }
}
