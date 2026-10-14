namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Functions of sentiment forecast from quota.</summary>
public static class SentimentForecastFromQuota
{
    public static Dictionary<string, double> SummarizeSentimentQuotasForForecast(List<SentimentQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<SentimentForecast> DeriveSentimentForecastsFromQuota(List<SentimentQuota> items)
    {
        var outValue = new List<SentimentForecast>();
        foreach (var item in items)
        {
            var derived = SentimentForecast.NewSentimentForecast(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeSentimentForecast(SentimentForecast item)
    {
        return SentimentThresholdPolicy.ClassifySentimentThreshold(item.MeasuredValue());
    }
}
