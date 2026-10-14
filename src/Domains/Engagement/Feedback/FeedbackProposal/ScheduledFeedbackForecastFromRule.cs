namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Recognition.Preference;

/// <summary>Functions of scheduled feedback forecast from rule.</summary>
public static class ScheduledFeedbackForecastFromRule
{
    public static Dictionary<string, double> SummarizeRecognitionRulesForForecast(List<RecognitionRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<ScheduledFeedbackForecast> DeriveScheduledFeedbackForecastsFromRule(List<RecognitionRule> items)
    {
        var outValue = new List<ScheduledFeedbackForecast>();
        foreach (var item in items)
        {
            var derived = ScheduledFeedbackForecast.NewScheduledFeedbackForecast(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreScheduledFeedbackForecast(ScheduledFeedbackForecast item)
    {
        return RecognitionRulePolicy.NormalizeRecognitionRuleCode(item.Code);
    }
}
