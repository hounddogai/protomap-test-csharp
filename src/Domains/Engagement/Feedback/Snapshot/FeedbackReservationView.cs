namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a feedback reservation.</summary>
public sealed record FeedbackReservationView(string CodeText, string Label, string SalaryBand, string ReviewerText, string EndsAt, string Maximum, string Priority, string StatusLabel)
{
    public static FeedbackReservationView ToFeedbackReservationView(FeedbackReservation item)
    {
        return new FeedbackReservationView(
            item.Code,
            item.Label,
            item.SalaryBand,
            item.Reviewer,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackReservationView> ToFeedbackReservationViews(List<FeedbackReservation> items)
    {
        var views = new List<FeedbackReservationView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackReservationView(item));
        }
        return views;
    }

    public static double SumFeedbackReservationView(FeedbackReservationView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedMaximum + (double)parsedPriority;
    }
}
