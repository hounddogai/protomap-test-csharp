namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Functions of skill profile target from forecast.</summary>
public static class SkillProfileTargetFromForecast
{
    public static Dictionary<string, double> SummarizeBatchedSkillProfileForecastsForTarget(List<BatchedSkillProfileForecast> items)
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

    public static List<SkillProfileTarget> DeriveSkillProfileTargetsFromForecast(List<BatchedSkillProfileForecast> items)
    {
        var outValue = new List<SkillProfileTarget>();
        foreach (var item in items)
        {
            var derived = SkillProfileTarget.NewSkillProfileTarget(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchSkillProfileTarget(SkillProfileTarget item)
    {
        return BatchedSkillProfileForecastPolicy.NormalizeBatchedSkillProfileForecastCode(item.Code);
    }
}
