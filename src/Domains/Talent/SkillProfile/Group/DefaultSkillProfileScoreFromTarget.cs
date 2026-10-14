namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>Functions of default skill profile score from target.</summary>
public static class DefaultSkillProfileScoreFromTarget
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

    public static List<DefaultSkillProfileScore> DeriveDefaultSkillProfileScoresFromTarget(List<SkillProfileTarget> items)
    {
        var outValue = new List<DefaultSkillProfileScore>();
        foreach (var item in items)
        {
            var derived = DefaultSkillProfileScore.NewDefaultSkillProfileScore(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeDefaultSkillProfileScore(DefaultSkillProfileScore item)
    {
        return SkillProfileThresholdPolicy.SkillProfileThresholdWeight(6, true);
    }
}
