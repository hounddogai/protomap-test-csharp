namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Functions of scenario template from digest.</summary>
public static class ScenarioTemplateFromDigest
{
    public static Dictionary<string, double> SummarizeSeasonalSpanDigestsForTemplate(List<SeasonalSpanDigest> items)
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

    public static List<ScenarioTemplate> DeriveScenarioTemplatesFromDigest(List<SeasonalSpanDigest> items)
    {
        var outValue = new List<ScenarioTemplate>();
        foreach (var item in items)
        {
            var derived = ScenarioTemplate.NewScenarioTemplate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveScenarioTemplate(ScenarioTemplate item)
    {
        return SpanBaselinePolicy.SpanBaselineWeight(7, true);
    }
}
