namespace Acme.Workforce.Workplace.Booking.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking adjustment.</summary>
public sealed record BookingAdjustmentView(string CodeText, string Label, string Segment, string Minimum, string Currency, string Active, string CreatedAt, string Version, string UpdatedAt, string LeaveDaysText, string StatusLabel)
{
    public static BookingAdjustmentView ToBookingAdjustmentView(BookingAdjustment item)
    {
        return new BookingAdjustmentView(
            item.Code,
            item.Label,
            Text.Truncate(item.Segment, 64),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 64),
            $"{item.Active}",
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingAdjustmentView> ToBookingAdjustmentViews(List<BookingAdjustment> items)
    {
        var views = new List<BookingAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToBookingAdjustmentView(item));
        }
        return views;
    }

    public static double SumBookingAdjustmentView(BookingAdjustmentView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedCreatedAt + (double)parsedVersion;
    }
}
