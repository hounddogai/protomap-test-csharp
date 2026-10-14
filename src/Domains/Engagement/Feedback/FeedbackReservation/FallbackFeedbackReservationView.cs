namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback feedback reservation.</summary>
public sealed record FallbackFeedbackReservationView(string Code, string Label, string WindowDays, string ScoreText, string Checksum, string Name, string Version, string Notes, string CreatedAtText, string Badge, string StatusLabel)
{
    public static FallbackFeedbackReservationView ToFallbackFeedbackReservationView(FallbackFeedbackReservation item)
    {
        return new FallbackFeedbackReservationView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 40),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackFeedbackReservationView> ToFallbackFeedbackReservationViews(List<FallbackFeedbackReservation> items)
    {
        var views = new List<FallbackFeedbackReservationView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackFeedbackReservationView(item));
        }
        return views;
    }

    public static double SumFallbackFeedbackReservationView(FallbackFeedbackReservationView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedScore + (double)parsedVersion;
    }
}
