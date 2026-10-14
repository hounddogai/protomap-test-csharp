namespace Acme.Workforce.Engagement.Feedback.FeedbackGroup;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Functions of capped feedback group from variance.</summary>
public static class CappedFeedbackGroupFromVariance
{
    public static Dictionary<string, double> SummarizePrimaryRecognitionVariancesForGroup(List<PrimaryRecognitionVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<CappedFeedbackGroup> DeriveCappedFeedbackGroupsFromVariance(List<PrimaryRecognitionVariance> items)
    {
        var outValue = new List<CappedFeedbackGroup>();
        foreach (var item in items)
        {
            var derived = CappedFeedbackGroup.NewCappedFeedbackGroup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleCappedFeedbackGroup(CappedFeedbackGroup item)
    {
        return ManualRecognitionSchedulePolicy.ClassifyManualRecognitionSchedule(item.MeasuredValue());
    }
}
