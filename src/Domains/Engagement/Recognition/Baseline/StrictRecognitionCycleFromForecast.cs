namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Functions of strict recognition cycle from forecast.</summary>
public static class StrictRecognitionCycleFromForecast
{
    public static Dictionary<string, double> SummarizeAdaptiveFeedbackForecastsForCycle(List<AdaptiveFeedbackForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<StrictRecognitionCycle> DeriveStrictRecognitionCyclesFromForecast(List<AdaptiveFeedbackForecast> items)
    {
        var outValue = new List<StrictRecognitionCycle>();
        foreach (var item in items)
        {
            var derived = StrictRecognitionCycle.NewStrictRecognitionCycle(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeStrictRecognitionCycle(StrictRecognitionCycle item)
    {
        return StrictFeedbackQuotaPolicy.StrictFeedbackQuotaBucket(item.ProjectedTotal());
    }
}
