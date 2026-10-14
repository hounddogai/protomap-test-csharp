namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Engagement.Feedback.Baseline;

/// <summary>Functions of recognition cycle from baseline.</summary>
public static class RecognitionCycleFromBaseline
{
    public static Dictionary<string, double> SummarizeScheduledFeedbackBaselinesForCycle(List<ScheduledFeedbackBaseline> items)
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

    public static List<RecognitionCycle> DeriveRecognitionCyclesFromBaseline(List<ScheduledFeedbackBaseline> items)
    {
        var outValue = new List<RecognitionCycle>();
        foreach (var item in items)
        {
            var derived = RecognitionCycle.NewRecognitionCycle(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveRecognitionCycle(RecognitionCycle item)
    {
        return FeedbackSummaryPolicy.NormalizeFeedbackSummaryCode(item.Code);
    }
}
