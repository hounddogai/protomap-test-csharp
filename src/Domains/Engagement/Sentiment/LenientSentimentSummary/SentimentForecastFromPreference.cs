namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>Functions of sentiment forecast from preference.</summary>
public static class SentimentForecastFromPreference
{
    public static Dictionary<string, double> SummarizeDefaultActionPlanPreferencesForForecast(List<DefaultActionPlanPreference> items)
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

    public static List<SentimentForecast> DeriveSentimentForecastsFromPreference(List<DefaultActionPlanPreference> items)
    {
        var outValue = new List<SentimentForecast>();
        foreach (var item in items)
        {
            var derived = SentimentForecast.NewSentimentForecast(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignSentimentForecast(SentimentForecast item)
    {
        return ActionPlanPolicyPolicy.ActionPlanPolicyWeight(4, true);
    }
}
