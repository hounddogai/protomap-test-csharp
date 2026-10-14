namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Functions of manual recognition schedule from assignment.</summary>
public static class ManualRecognitionScheduleFromAssignment
{
    public static Dictionary<string, double> SummarizeFeedbackAssignmentsForSchedule(List<FeedbackAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<ManualRecognitionSchedule> DeriveManualRecognitionSchedulesFromAssignment(List<FeedbackAssignment> items)
    {
        var outValue = new List<ManualRecognitionSchedule>();
        foreach (var item in items)
        {
            var derived = ManualRecognitionSchedule.NewManualRecognitionSchedule(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleManualRecognitionSchedule(ManualRecognitionSchedule item)
    {
        return BatchedFeedbackBaselinePolicy.BatchedFeedbackBaselineBucket(item.MeasuredValue());
    }
}
