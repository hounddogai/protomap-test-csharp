namespace Acme.Workforce.Engagement.Feedback.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled feedback baseline.</summary>
public sealed record ScheduledFeedbackBaselineView(string Code, string Label, string ExternalRef, string CeilingText, string ScoreText, string Currency, string StatusLabel)
{
    public static ScheduledFeedbackBaselineView ToScheduledFeedbackBaselineView(ScheduledFeedbackBaseline item)
    {
        return new ScheduledFeedbackBaselineView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.ExternalRef,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledFeedbackBaselineView> ToScheduledFeedbackBaselineViews(List<ScheduledFeedbackBaseline> items)
    {
        var views = new List<ScheduledFeedbackBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledFeedbackBaselineView(item));
        }
        return views;
    }

    public static double SumScheduledFeedbackBaselineView(ScheduledFeedbackBaselineView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedScore;
    }
}
