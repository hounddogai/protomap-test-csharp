namespace Acme.Workforce.Engagement.TownHall.Window;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Functions of town hall window from template.</summary>
public static class TownHallWindowFromTemplate
{
    public static Dictionary<string, double> SummarizeFeedbackTemplatesForWindow(List<FeedbackTemplate> items)
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

    public static List<TownHallWindow> DeriveTownHallWindowsFromTemplate(List<FeedbackTemplate> items)
    {
        var outValue = new List<TownHallWindow>();
        foreach (var item in items)
        {
            var derived = TownHallWindow.NewTownHallWindow(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeTownHallWindow(TownHallWindow item)
    {
        return RollingFeedbackThresholdPolicy.RollingFeedbackThresholdBucket(item.NetExposure());
    }
}
