namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking snapshot.</summary>
public sealed record BookingSnapshotView(string CodeText, string NameText, string ApprovedText, string Notes, string Description, string DurationMinutes, string Summary, string StatusLabel)
{
    public static BookingSnapshotView ToBookingSnapshotView(BookingSnapshot item)
    {
        return new BookingSnapshotView(
            item.Code,
            item.Name,
            $"{item.Approved}",
            Text.Truncate(item.Notes, 40),
            item.Description,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingSnapshotView> ToBookingSnapshotViews(List<BookingSnapshot> items)
    {
        var views = new List<BookingSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToBookingSnapshotView(item));
        }
        return views;
    }

    public static double SumBookingSnapshotView(BookingSnapshotView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes;
    }
}
