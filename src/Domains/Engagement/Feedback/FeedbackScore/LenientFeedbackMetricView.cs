namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient feedback metric.</summary>
public sealed record LenientFeedbackMetricView(string Code, string Title, string JobCodeText, string NotesText, string UpdatedAt, string PriorityText, string Segment, string Channel, string StatusLabel)
{
    public static LenientFeedbackMetricView ToLenientFeedbackMetricView(LenientFeedbackMetric item)
    {
        return new LenientFeedbackMetricView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.JobCode,
            item.Notes,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientFeedbackMetricView> ToLenientFeedbackMetricViews(List<LenientFeedbackMetric> items)
    {
        var views = new List<LenientFeedbackMetricView>();
        foreach (var item in items)
        {
            views.Add(ToLenientFeedbackMetricView(item));
        }
        return views;
    }

    public static double SumLenientFeedbackMetricView(LenientFeedbackMetricView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedPriority;
    }
}
