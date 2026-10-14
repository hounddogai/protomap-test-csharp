namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Functions of reorg metric from exception.</summary>
public static class ReorgMetricFromException
{
    public static Dictionary<string, double> SummarizeScenarioExceptionsForMetric(List<ScenarioException> items)
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

    public static List<ReorgMetric> DeriveReorgMetricsFromException(List<ScenarioException> items)
    {
        var outValue = new List<ReorgMetric>();
        foreach (var item in items)
        {
            var derived = ReorgMetric.NewReorgMetric(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyReorgMetric(ReorgMetric item)
    {
        return ManualScenarioAuditPolicy.ManualScenarioAuditWeight(3, true);
    }
}
