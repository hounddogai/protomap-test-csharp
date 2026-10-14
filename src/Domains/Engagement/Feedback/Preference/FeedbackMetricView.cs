namespace Acme.Workforce.Engagement.Feedback.Preference;

using System.Globalization;

/// <summary>How screens and exports show a feedback metric.</summary>
public sealed record FeedbackMetricView(string Code, string Title, string Name, string Category, string Summary, string Quantity, string StatusLabel)
{
    public static FeedbackMetricView ToFeedbackMetricView(FeedbackMetric item)
    {
        return new FeedbackMetricView(
            item.Code,
            item.Title,
            item.Name,
            item.Category,
            item.Summary,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackMetricView> ToFeedbackMetricViews(List<FeedbackMetric> items)
    {
        var views = new List<FeedbackMetricView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackMetricView(item));
        }
        return views;
    }

    public static double SumFeedbackMetricView(FeedbackMetricView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
