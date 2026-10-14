namespace Acme.Workforce.Workplace.Booking.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking preference.</summary>
public sealed record BookingPreferenceView(string Code, string LabelText, string Grade, string AttemptsText, string Visible, string SummaryText, string CostText, string ScoreText, string Title, string StatusLabel)
{
    public static BookingPreferenceView ToBookingPreferenceView(BookingPreference item)
    {
        return new BookingPreferenceView(
            item.Code,
            item.Label,
            Text.Truncate(item.Grade, 80),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Summary,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingPreferenceView> ToBookingPreferenceViews(List<BookingPreference> items)
    {
        var views = new List<BookingPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToBookingPreferenceView(item));
        }
        return views;
    }

    public static double SumBookingPreferenceView(BookingPreferenceView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedCost + parsedScore;
    }
}
