namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled booking baseline.</summary>
public sealed record ScheduledBookingBaselineView(string CodeText, string TitleText, string Team, string Summary, string Archived, string Limit, string DurationMinutesText, string StatusLabel)
{
    public static ScheduledBookingBaselineView ToScheduledBookingBaselineView(ScheduledBookingBaseline item)
    {
        return new ScheduledBookingBaselineView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Team,
            Text.Truncate(item.Summary, 64),
            $"{item.Archived}",
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledBookingBaselineView> ToScheduledBookingBaselineViews(List<ScheduledBookingBaseline> items)
    {
        var views = new List<ScheduledBookingBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledBookingBaselineView(item));
        }
        return views;
    }

    public static double SumScheduledBookingBaselineView(ScheduledBookingBaselineView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedDurationMinutes;
    }
}
