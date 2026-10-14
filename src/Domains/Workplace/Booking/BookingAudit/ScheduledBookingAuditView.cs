namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using System.Globalization;

/// <summary>How screens and exports show a scheduled booking audit.</summary>
public sealed record ScheduledBookingAuditView(string Code, string Title, string Cost, string SummaryText, string StatusLabel)
{
    public static ScheduledBookingAuditView ToScheduledBookingAuditView(ScheduledBookingAudit item)
    {
        return new ScheduledBookingAuditView(
            item.Code,
            item.Title,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledBookingAuditView> ToScheduledBookingAuditViews(List<ScheduledBookingAudit> items)
    {
        var views = new List<ScheduledBookingAuditView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledBookingAuditView(item));
        }
        return views;
    }

    public static double SumScheduledBookingAuditView(ScheduledBookingAuditView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
