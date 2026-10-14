namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>Functions of scheduled sentiment target from digest.</summary>
public static class ScheduledSentimentTargetFromDigest
{
    public static Dictionary<string, double> SummarizePulseSurveyDigestsForTarget(List<PulseSurveyDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<ScheduledSentimentTarget> DeriveScheduledSentimentTargetsFromDigest(List<PulseSurveyDigest> items)
    {
        var outValue = new List<ScheduledSentimentTarget>();
        foreach (var item in items)
        {
            var derived = ScheduledSentimentTarget.NewScheduledSentimentTarget(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreScheduledSentimentTarget(ScheduledSentimentTarget item)
    {
        return PulseSurveySummaryPolicy.ClassifyPulseSurveySummary(item.WeightedTotal());
    }
}
