namespace Acme.Workforce.Orgdesign.Scenario.Tier;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Functions of scenario baseline from target.</summary>
public static class ScenarioBaselineFromTarget
{
    public static Dictionary<string, double> SummarizeScenarioTargetsForBaseline(List<ScenarioTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<ScenarioBaseline> DeriveScenarioBaselinesFromTarget(List<ScenarioTarget> items)
    {
        var outValue = new List<ScenarioBaseline>();
        foreach (var item in items)
        {
            var derived = ScenarioBaseline.NewScenarioBaseline(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreScenarioBaseline(ScenarioBaseline item)
    {
        return ScenarioTargetPolicy.ScenarioTargetBucket(item.ProjectedTotal());
    }
}
