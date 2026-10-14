namespace Acme.Workforce.Workplace.Booking.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking template.</summary>
public sealed record BookingTemplateView(string CodeText, string Name, string TeamText, string Ceiling, string Hours, string Unit, string StatusLabel)
{
    public static BookingTemplateView ToBookingTemplateView(BookingTemplate item)
    {
        return new BookingTemplateView(
            item.Code,
            item.Name,
            Text.Truncate(item.Team, 40),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingTemplateView> ToBookingTemplateViews(List<BookingTemplate> items)
    {
        var views = new List<BookingTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToBookingTemplateView(item));
        }
        return views;
    }

    public static double SumBookingTemplateView(BookingTemplateView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedHours;
    }
}
