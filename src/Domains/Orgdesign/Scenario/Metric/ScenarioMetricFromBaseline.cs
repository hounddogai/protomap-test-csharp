namespace Acme.Workforce.Orgdesign.Scenario.Metric;

using Acme.Workforce.Orgdesign.Scenario.Tier;

/// <summary>Functions of scenario metric from baseline.</summary>
public static class ScenarioMetricFromBaseline
{
    public static Dictionary<string, double> SummarizeScenarioBaselinesForMetric(List<ScenarioBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<ScenarioMetric> DeriveScenarioMetricsFromBaseline(List<ScenarioBaseline> items)
    {
        var outValue = new List<ScenarioMetric>();
        foreach (var item in items)
        {
            var derived = ScenarioMetric.NewScenarioMetric(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreScenarioMetric(ScenarioMetric item)
    {
        return SeasonalScenarioTierPolicy.SeasonalScenarioTierWeight(1, true);
    }
}
