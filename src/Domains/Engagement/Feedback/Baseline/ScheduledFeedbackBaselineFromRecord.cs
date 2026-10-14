namespace Acme.Workforce.Engagement.Feedback.Baseline;

using Acme.Workforce.Engagement.Feedback.RecordItem;

/// <summary>Functions of scheduled feedback baseline from record.</summary>
public static class ScheduledFeedbackBaselineFromRecord
{
    public static Dictionary<string, double> SummarizeFeedbackRecordsForBaseline(List<FeedbackRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<ScheduledFeedbackBaseline> DeriveScheduledFeedbackBaselinesFromRecord(List<FeedbackRecord> items)
    {
        var outValue = new List<ScheduledFeedbackBaseline>();
        foreach (var item in items)
        {
            var derived = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }
}
