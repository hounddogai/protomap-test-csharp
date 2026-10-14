namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Engagement.Sentiment.Manifest;

/// <summary>Functions of recognition preference from target.</summary>
public static class RecognitionPreferenceFromTarget
{
    public static Dictionary<string, double> SummarizeScheduledSentimentTargetsForPreference(List<ScheduledSentimentTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<RecognitionPreference> DeriveRecognitionPreferencesFromTarget(List<ScheduledSentimentTarget> items)
    {
        var outValue = new List<RecognitionPreference>();
        foreach (var item in items)
        {
            var derived = RecognitionPreference.NewRecognitionPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveRecognitionPreference(RecognitionPreference item)
    {
        return ScheduledSentimentTargetPolicy.ClassifyScheduledSentimentTarget(item.NetExposure());
    }
}
