namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled adjustment reservation.</summary>
public sealed record ScheduledAdjustmentReservationView(string Code, string Title, string Priority, string Phone, string Reviewer, string OwnerId, string Ceiling, string GraceDays, string Channel, string StatusLabel)
{
    public static ScheduledAdjustmentReservationView ToScheduledAdjustmentReservationView(ScheduledAdjustmentReservation item)
    {
        return new ScheduledAdjustmentReservationView(
            item.Code,
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            Text.Truncate(item.Reviewer, 80),
            item.OwnerId,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAdjustmentReservationView> ToScheduledAdjustmentReservationViews(List<ScheduledAdjustmentReservation> items)
    {
        var views = new List<ScheduledAdjustmentReservationView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAdjustmentReservationView(item));
        }
        return views;
    }

    public static double SumScheduledAdjustmentReservationView(ScheduledAdjustmentReservationView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedCeiling + (double)parsedGraceDays;
    }
}
