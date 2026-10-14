namespace Acme.Workforce.Workplace.Booking.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking manifest.</summary>
public sealed record BookingManifestView(string Code, string NameText, string Archived, string JobCode, string EndsAt, string Department, string FloorValue, string StatusLabel)
{
    public static BookingManifestView ToBookingManifestView(BookingManifest item)
    {
        return new BookingManifestView(
            item.Code,
            Text.Truncate(item.Name, 120),
            $"{item.Archived}",
            Text.Truncate(item.JobCode, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingManifestView> ToBookingManifestViews(List<BookingManifest> items)
    {
        var views = new List<BookingManifestView>();
        foreach (var item in items)
        {
            views.Add(ToBookingManifestView(item));
        }
        return views;
    }

    public static double SumBookingManifestView(BookingManifestView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedFloorValue;
    }
}
