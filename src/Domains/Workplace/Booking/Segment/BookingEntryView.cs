namespace Acme.Workforce.Workplace.Booking.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking entry.</summary>
public sealed record BookingEntryView(string Code, string Title, string Attempts, string DurationMinutesText, string Segment, string Fte, string Summary, string Ceiling, string Category, string LeaveDays, string StatusLabel)
{
    public static BookingEntryView ToBookingEntryView(BookingEntry item)
    {
        return new BookingEntryView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingEntryView> ToBookingEntryViews(List<BookingEntry> items)
    {
        var views = new List<BookingEntryView>();
        foreach (var item in items)
        {
            views.Add(ToBookingEntryView(item));
        }
        return views;
    }

    public static double SumBookingEntryView(BookingEntryView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedDurationMinutes + parsedFte;
    }
}
