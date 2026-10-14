namespace Acme.Workforce.Engagement.Feedback.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback summary.</summary>
public sealed record FeedbackSummaryView(string Code, string Label, string ExpiresAt, string Pinned, string Margin, string Capacity, string Reviewer, string EndsAtText, string SkillText, string Priority, string StatusLabel)
{
    public static FeedbackSummaryView ToFeedbackSummaryView(FeedbackSummary item)
    {
        return new FeedbackSummaryView(
            item.Code,
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackSummaryView> ToFeedbackSummaryViews(List<FeedbackSummary> items)
    {
        var views = new List<FeedbackSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackSummaryView(item));
        }
        return views;
    }

    public static double SumFeedbackSummaryView(FeedbackSummaryView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedMargin + (double)parsedCapacity;
    }
}
