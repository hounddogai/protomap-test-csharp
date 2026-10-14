namespace Acme.Workforce.Workplace.Booking.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled booking template.</summary>
public sealed record ScheduledBookingTemplateView(string CodeText, string Name, string LocationText, string Quantity, string GradeText, string Title, string Score, string ArchivedText, string StatusLabel)
{
    public static ScheduledBookingTemplateView ToScheduledBookingTemplateView(ScheduledBookingTemplate item)
    {
        return new ScheduledBookingTemplateView(
            item.Code,
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.Location, 64),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 80),
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledBookingTemplateView> ToScheduledBookingTemplateViews(List<ScheduledBookingTemplate> items)
    {
        var views = new List<ScheduledBookingTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledBookingTemplateView(item));
        }
        return views;
    }

    public static double SumScheduledBookingTemplateView(ScheduledBookingTemplateView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedScore;
    }
}
