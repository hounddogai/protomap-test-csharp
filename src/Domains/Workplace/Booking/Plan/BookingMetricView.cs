namespace Acme.Workforce.Workplace.Booking.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking metric.</summary>
public sealed record BookingMetricView(string Code, string Name, string Retries, string NotesText, string ManagerId, string Category, string Version, string AmountText, string StatusLabel)
{
    public static BookingMetricView ToBookingMetricView(BookingMetric item)
    {
        return new BookingMetricView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 64),
            item.ManagerId,
            item.Category,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingMetricView> ToBookingMetricViews(List<BookingMetric> items)
    {
        var views = new List<BookingMetricView>();
        foreach (var item in items)
        {
            views.Add(ToBookingMetricView(item));
        }
        return views;
    }

    public static double SumBookingMetricView(BookingMetricView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedVersion + parsedAmount;
    }
}
