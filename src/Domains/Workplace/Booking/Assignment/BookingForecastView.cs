namespace Acme.Workforce.Workplace.Booking.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking forecast.</summary>
public sealed record BookingForecastView(string Code, string Title, string Currency, string Weight, string Score, string VisibleText, string CostCenterText, string Minimum, string Cost, string BadgeText, string StatusLabel)
{
    public static BookingForecastView ToBookingForecastView(BookingForecast item)
    {
        return new BookingForecastView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Currency,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.CostCenter,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingForecastView> ToBookingForecastViews(List<BookingForecast> items)
    {
        var views = new List<BookingForecastView>();
        foreach (var item in items)
        {
            views.Add(ToBookingForecastView(item));
        }
        return views;
    }

    public static double SumBookingForecastView(BookingForecastView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedWeight + parsedScore + parsedMinimum;
    }
}
