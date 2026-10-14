namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Functions of scenario score from target.</summary>
public static class ScenarioScoreFromTarget
{
    public static Dictionary<string, double> SummarizeReorgTargetsForScore(List<ReorgTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<ScenarioScore> DeriveScenarioScoresFromTarget(List<ReorgTarget> items)
    {
        var outValue = new List<ScenarioScore>();
        foreach (var item in items)
        {
            var derived = ScenarioScore.NewScenarioScore(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignScenarioScore(ScenarioScore item)
    {
        return ReorgVariancePolicy.ClassifyReorgVariance(item.NetExposure());
    }
}
