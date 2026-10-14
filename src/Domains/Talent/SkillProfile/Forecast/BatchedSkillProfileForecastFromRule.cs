namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Talent.Nomination.Target;

/// <summary>Functions of batched skill profile forecast from rule.</summary>
public static class BatchedSkillProfileForecastFromRule
{
    public static Dictionary<string, double> SummarizeTieredNominationRulesForForecast(List<TieredNominationRule> items)
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

    public static List<BatchedSkillProfileForecast> DeriveBatchedSkillProfileForecastsFromRule(List<TieredNominationRule> items)
    {
        var outValue = new List<BatchedSkillProfileForecast>();
        foreach (var item in items)
        {
            var derived = BatchedSkillProfileForecast.NewBatchedSkillProfileForecast(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchBatchedSkillProfileForecast(BatchedSkillProfileForecast item)
    {
        return TieredNominationRulePolicy.ClassifyTieredNominationRule(item.ProjectedTotal());
    }
}
