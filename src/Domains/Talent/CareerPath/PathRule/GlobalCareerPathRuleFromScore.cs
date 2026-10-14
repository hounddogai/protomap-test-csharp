namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Functions of global career path rule from score.</summary>
public static class GlobalCareerPathRuleFromScore
{
    public static Dictionary<string, double> SummarizeStrictSuccessionPlanScoresForRule(List<StrictSuccessionPlanScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<GlobalCareerPathRule> DeriveGlobalCareerPathRulesFromScore(List<StrictSuccessionPlanScore> items)
    {
        var outValue = new List<GlobalCareerPathRule>();
        foreach (var item in items)
        {
            var derived = GlobalCareerPathRule.NewGlobalCareerPathRule(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreGlobalCareerPathRule(GlobalCareerPathRule item)
    {
        return StrictSuccessionPlanScorePolicy.ClassifyStrictSuccessionPlanScore(item.NetLoad());
    }
}
