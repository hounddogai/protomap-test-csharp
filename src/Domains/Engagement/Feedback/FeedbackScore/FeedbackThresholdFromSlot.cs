namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Functions of feedback threshold from slot.</summary>
public static class FeedbackThresholdFromSlot
{
    public static Dictionary<string, double> SummarizeRollingRecognitionSlotsForThreshold(List<RollingRecognitionSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<FeedbackThreshold> DeriveFeedbackThresholdsFromSlot(List<RollingRecognitionSlot> items)
    {
        var outValue = new List<FeedbackThreshold>();
        foreach (var item in items)
        {
            var derived = FeedbackThreshold.NewFeedbackThreshold(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveFeedbackThreshold(FeedbackThreshold item)
    {
        return RecognitionSchedulePolicy.RecognitionScheduleWeight(4, true);
    }
}
