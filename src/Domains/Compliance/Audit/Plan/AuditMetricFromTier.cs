namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Compliance.Exception.Estimate;

/// <summary>Functions of audit metric from tier.</summary>
public static class AuditMetricFromTier
{
    public static Dictionary<string, double> SummarizeExceptionTiersForMetric(List<ExceptionTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<AuditMetric> DeriveAuditMetricsFromTier(List<ExceptionTier> items)
    {
        var outValue = new List<AuditMetric>();
        foreach (var item in items)
        {
            var derived = AuditMetric.NewAuditMetric(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankAuditMetric(AuditMetric item)
    {
        return ExceptionEstimatePolicy.ClassifyExceptionEstimate(item.WeightedExposure());
    }
}
