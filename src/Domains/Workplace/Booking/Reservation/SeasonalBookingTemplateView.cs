namespace Acme.Workforce.Workplace.Booking.Reservation;

using System.Globalization;

/// <summary>How screens and exports show a seasonal booking template.</summary>
public sealed record SeasonalBookingTemplateView(string Code, string Title, string Hours, string Segment, string Quantity, string TenureMonths, string Badge, string StatusLabel)
{
    public static SeasonalBookingTemplateView ToSeasonalBookingTemplateView(SeasonalBookingTemplate item)
    {
        return new SeasonalBookingTemplateView(
            item.Code,
            item.Title,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalBookingTemplateView> ToSeasonalBookingTemplateViews(List<SeasonalBookingTemplate> items)
    {
        var views = new List<SeasonalBookingTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalBookingTemplateView(item));
        }
        return views;
    }

    public static double SumSeasonalBookingTemplateView(SeasonalBookingTemplateView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedQuantity + (double)parsedTenureMonths;
    }
}
