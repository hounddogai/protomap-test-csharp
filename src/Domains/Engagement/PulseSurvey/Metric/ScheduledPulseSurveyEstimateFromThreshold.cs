namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Engagement.Recognition.Threshold;
using Acme.Workforce.Engagement.Sentiment.Threshold;

/// <summary>Functions of scheduled pulse survey estimate from threshold.</summary>
public static class ScheduledPulseSurveyEstimateFromThreshold
{
    public static Dictionary<string, double> SummarizeLenientSentimentThresholdsForEstimate(List<LenientSentimentThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<ScheduledPulseSurveyEstimate> DeriveScheduledPulseSurveyEstimatesFromThreshold(List<LenientSentimentThreshold> items)
    {
        var outValue = new List<ScheduledPulseSurveyEstimate>();
        foreach (var item in items)
        {
            var derived = ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeScheduledPulseSurveyEstimate(ScheduledPulseSurveyEstimate item)
    {
        return LenientSentimentThresholdPolicy.LenientSentimentThresholdWeight(9, true);
    }

    public static Dictionary<string, double> SummarizeRegionalRecognitionThresholdsForEstimate(List<RegionalRecognitionThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<ScheduledPulseSurveyEstimate> DeriveScheduledPulseSurveyEstimatesFromThresholdExtra(List<RegionalRecognitionThreshold> items)
    {
        var outValue = new List<ScheduledPulseSurveyEstimate>();
        foreach (var item in items)
        {
            var derived = ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateScheduledPulseSurveyEstimate(ScheduledPulseSurveyEstimate item)
    {
        return AdaptiveRecognitionBatchPolicy.AdaptiveRecognitionBatchBucket(item.WeightedTotal());
    }
}
