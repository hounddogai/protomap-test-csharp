namespace Acme.Workforce.Workplace.Booking.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking digest.</summary>
public sealed record BookingDigestView(string CodeText, string Name, string EmployeeId, string GraceDays, string CapacityText, string Locale, string StatusLabel)
{
    public static BookingDigestView ToBookingDigestView(BookingDigest item)
    {
        return new BookingDigestView(
            item.Code,
            item.Name,
            item.EmployeeId,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingDigestView> ToBookingDigestViews(List<BookingDigest> items)
    {
        var views = new List<BookingDigestView>();
        foreach (var item in items)
        {
            views.Add(ToBookingDigestView(item));
        }
        return views;
    }

    public static double SumBookingDigestView(BookingDigestView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedCapacity;
    }
}
