namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Functions of tiered sentiment target from rollup.</summary>
public static class TieredSentimentTargetFromRollup
{
    public static Dictionary<string, double> SummarizePulseSurveyRollupsForTarget(List<PulseSurveyRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<TieredSentimentTarget> DeriveTieredSentimentTargetsFromRollup(List<PulseSurveyRollup> items)
    {
        var outValue = new List<TieredSentimentTarget>();
        foreach (var item in items)
        {
            var derived = TieredSentimentTarget.NewTieredSentimentTarget(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveTieredSentimentTarget(TieredSentimentTarget item)
    {
        return PulseSurveyRollupPolicy.ClassifyPulseSurveyRollup(item.AdjustedLoad());
    }
}
