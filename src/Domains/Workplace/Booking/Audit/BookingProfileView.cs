namespace Acme.Workforce.Workplace.Booking.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking profile.</summary>
public sealed record BookingProfileView(string Code, string LabelText, string Revision, string Visible, string OwnerId, string UnitText, string CapacityText, string ExpiresAtText, string Currency, string DiscountText, string Approved, string StatusLabel)
{
    public static BookingProfileView ToBookingProfileView(BookingProfile item)
    {
        return new BookingProfileView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.OwnerId,
            item.Unit,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 64),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingProfileView> ToBookingProfileViews(List<BookingProfile> items)
    {
        var views = new List<BookingProfileView>();
        foreach (var item in items)
        {
            views.Add(ToBookingProfileView(item));
        }
        return views;
    }

    public static double SumBookingProfileView(BookingProfileView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedCapacity + (double)parsedExpiresAt;
    }
}
