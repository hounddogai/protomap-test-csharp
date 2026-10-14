namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Functions of scenario manifest from baseline.</summary>
public static class ScenarioManifestFromBaseline
{
    public static Dictionary<string, double> SummarizePendingSpanBaselinesForManifest(List<PendingSpanBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<ScenarioManifest> DeriveScenarioManifestsFromBaseline(List<PendingSpanBaseline> items)
    {
        var outValue = new List<ScenarioManifest>();
        foreach (var item in items)
        {
            var derived = ScenarioManifest.NewScenarioManifest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyScenarioManifest(ScenarioManifest item)
    {
        return PendingSpanBaselinePolicy.PendingSpanBaselineBucket(item.EffectiveLoad());
    }
}
