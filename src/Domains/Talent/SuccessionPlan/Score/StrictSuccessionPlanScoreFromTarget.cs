namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>Functions of strict succession plan score from target.</summary>
public static class StrictSuccessionPlanScoreFromTarget
{
    public static Dictionary<string, double> SummarizeSkillProfileTargetsForScore(List<SkillProfileTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<StrictSuccessionPlanScore> DeriveStrictSuccessionPlanScoresFromTarget(List<SkillProfileTarget> items)
    {
        var outValue = new List<StrictSuccessionPlanScore>();
        foreach (var item in items)
        {
            var derived = StrictSuccessionPlanScore.NewStrictSuccessionPlanScore(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitStrictSuccessionPlanScore(StrictSuccessionPlanScore item)
    {
        return SkillProfileThresholdPolicy.SkillProfileThresholdWeight(7, true);
    }
}
