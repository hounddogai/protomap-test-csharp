namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback exception.</summary>
public sealed record FeedbackExceptionView(string CodeText, string Label, string DurationMinutes, string UpdatedAtText, string StartsAt, string Fte, string UnitText, string Summary, string Team, string Limit, string StatusLabel)
{
    public static FeedbackExceptionView ToFeedbackExceptionView(FeedbackException item)
    {
        return new FeedbackExceptionView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.Summary,
            item.Team,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackExceptionView> ToFeedbackExceptionViews(List<FeedbackException> items)
    {
        var views = new List<FeedbackExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackExceptionView(item));
        }
        return views;
    }

    public static double SumFeedbackExceptionView(FeedbackExceptionView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedUpdatedAt + (double)parsedStartsAt;
    }
}
