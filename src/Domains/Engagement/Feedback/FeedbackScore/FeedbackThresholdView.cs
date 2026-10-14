namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using System.Globalization;

/// <summary>How screens and exports show a feedback threshold.</summary>
public sealed record FeedbackThresholdView(string Code, string Label, string ChannelText, string CostCenter, string BadgeText, string PriorityText, string PinnedText, string Unit, string CapacityText, string Locale, string StatusLabel)
{
    public static FeedbackThresholdView ToFeedbackThresholdView(FeedbackThreshold item)
    {
        return new FeedbackThresholdView(
            item.Code,
            item.Label,
            item.Channel,
            item.CostCenter,
            item.Badge,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Unit,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackThresholdView> ToFeedbackThresholdViews(List<FeedbackThreshold> items)
    {
        var views = new List<FeedbackThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackThresholdView(item));
        }
        return views;
    }

    public static double SumFeedbackThresholdView(FeedbackThresholdView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedCapacity;
    }
}
