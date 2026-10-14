namespace Acme.Workforce.Workplace.Booking.Segment;

using System.Globalization;

/// <summary>How screens and exports show a booking segment.</summary>
public sealed record BookingSegmentView(string Code, string Name, string CreatedAt, string LeaveDays, string Ceiling, string EmployeeId, string StatusLabel)
{
    public static BookingSegmentView ToBookingSegmentView(BookingSegment item)
    {
        return new BookingSegmentView(
            item.Code,
            item.Name,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingSegmentView> ToBookingSegmentViews(List<BookingSegment> items)
    {
        var views = new List<BookingSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToBookingSegmentView(item));
        }
        return views;
    }

    public static double SumBookingSegmentView(BookingSegmentView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedLeaveDays + parsedCeiling;
    }
}
