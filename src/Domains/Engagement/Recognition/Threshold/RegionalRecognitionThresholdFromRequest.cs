namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Functions of regional recognition threshold from request.</summary>
public static class RegionalRecognitionThresholdFromRequest
{
    public static Dictionary<string, double> SummarizeSentimentRequestsForThreshold(List<SentimentRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<RegionalRecognitionThreshold> DeriveRegionalRecognitionThresholdsFromRequest(List<SentimentRequest> items)
    {
        var outValue = new List<RegionalRecognitionThreshold>();
        foreach (var item in items)
        {
            var derived = RegionalRecognitionThreshold.NewRegionalRecognitionThreshold(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyRegionalRecognitionThresholdExtra(RegionalRecognitionThreshold item)
    {
        return SentimentMappingPolicy.ClassifySentimentMapping(item.MeasuredValue());
    }
}
