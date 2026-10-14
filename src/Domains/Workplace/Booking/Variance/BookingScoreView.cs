namespace Acme.Workforce.Workplace.Booking.Variance;

using System.Globalization;

/// <summary>How screens and exports show a booking score.</summary>
public sealed record BookingScoreView(string Code, string LabelText, string FteText, string Visible, string LeaveDays, string StatusLabel)
{
    public static BookingScoreView ToBookingScoreView(BookingScore item)
    {
        return new BookingScoreView(
            item.Code,
            item.Label,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingScoreView> ToBookingScoreViews(List<BookingScore> items)
    {
        var views = new List<BookingScoreView>();
        foreach (var item in items)
        {
            views.Add(ToBookingScoreView(item));
        }
        return views;
    }

    public static double SumBookingScoreView(BookingScoreView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedLeaveDays;
    }
}
