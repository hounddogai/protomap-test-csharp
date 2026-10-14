namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled feedback metric.</summary>
public sealed record ScheduledFeedbackMetricView(string Code, string Name, string Rate, string Summary, string UpdatedAtText, string Currency, string Ceiling, string RatioText, string VersionText, string Timezone, string Source, string StatusLabel)
{
    public static ScheduledFeedbackMetricView ToScheduledFeedbackMetricView(ScheduledFeedbackMetric item)
    {
        return new ScheduledFeedbackMetricView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 64),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            Text.Truncate(item.Source, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledFeedbackMetricView> ToScheduledFeedbackMetricViews(List<ScheduledFeedbackMetric> items)
    {
        var views = new List<ScheduledFeedbackMetricView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledFeedbackMetricView(item));
        }
        return views;
    }

    public static double SumScheduledFeedbackMetricView(ScheduledFeedbackMetricView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedUpdatedAt + parsedCeiling;
    }
}
