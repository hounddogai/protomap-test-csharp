namespace Acme.Workforce.Workplace.Booking.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking allocation.</summary>
public sealed record BookingAllocationView(string CodeText, string Label, string Phone, string Maximum, string HoursText, string Description, string NameText, string Category, string LimitText, string StatusLabel)
{
    public static BookingAllocationView ToBookingAllocationView(BookingAllocation item)
    {
        return new BookingAllocationView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Phone,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.Name, 64),
            item.Category,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingAllocationView> ToBookingAllocationViews(List<BookingAllocation> items)
    {
        var views = new List<BookingAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToBookingAllocationView(item));
        }
        return views;
    }

    public static double SumBookingAllocationView(BookingAllocationView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedHours + (double)parsedLimit;
    }
}
