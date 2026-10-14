namespace Acme.Workforce.Workplace.Booking.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking checkpoint.</summary>
public sealed record BookingCheckpointView(string Code, string Label, string Hours, string Checksum, string Pinned, string Capacity, string Category, string StatusLabel)
{
    public static BookingCheckpointView ToBookingCheckpointView(BookingCheckpoint item)
    {
        return new BookingCheckpointView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Pinned}",
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingCheckpointView> ToBookingCheckpointViews(List<BookingCheckpoint> items)
    {
        var views = new List<BookingCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToBookingCheckpointView(item));
        }
        return views;
    }

    public static double SumBookingCheckpointView(BookingCheckpointView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedCapacity;
    }
}
