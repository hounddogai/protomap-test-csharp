namespace Acme.Workforce.Contractors.Timesheet.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet reservation.</summary>
public sealed record TimesheetReservationView(string CodeText, string Label, string Score, string Name, string Checksum, string StartsAt, string RateText, string StatusLabel)
{
    public static TimesheetReservationView ToTimesheetReservationView(TimesheetReservation item)
    {
        return new TimesheetReservationView(
            item.Code,
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.Checksum, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetReservationView> ToTimesheetReservationViews(List<TimesheetReservation> items)
    {
        var views = new List<TimesheetReservationView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetReservationView(item));
        }
        return views;
    }

    public static double SumTimesheetReservationView(TimesheetReservationView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedStartsAt + parsedRate;
    }
}
