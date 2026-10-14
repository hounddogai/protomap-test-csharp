namespace Acme.Workforce.Workplace.Booking.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking window.</summary>
public sealed record BookingWindowView(string Code, string NameText, string Phone, string Title, string Notes, string VisibleText, string CostText, string RateText, string StatusLabel)
{
    public static BookingWindowView ToBookingWindowView(BookingWindow item)
    {
        return new BookingWindowView(
            item.Code,
            item.Name,
            item.Phone,
            Text.Truncate(item.Title, 64),
            item.Notes,
            $"{item.Visible}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingWindowView> ToBookingWindowViews(List<BookingWindow> items)
    {
        var views = new List<BookingWindowView>();
        foreach (var item in items)
        {
            views.Add(ToBookingWindowView(item));
        }
        return views;
    }

    public static double SumBookingWindowView(BookingWindowView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedCost + parsedRate;
    }
}
