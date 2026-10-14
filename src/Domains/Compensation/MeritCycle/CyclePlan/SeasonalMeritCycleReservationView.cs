namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal merit cycle reservation.</summary>
public sealed record SeasonalMeritCycleReservationView(string CodeText, string Title, string Threshold, string Notes, string Ceiling, string LeaveDays, string Grade, string Margin, string Summary, string Quantity, string StatusLabel)
{
    public static SeasonalMeritCycleReservationView ToSeasonalMeritCycleReservationView(SeasonalMeritCycleReservation item)
    {
        return new SeasonalMeritCycleReservationView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalMeritCycleReservationView> ToSeasonalMeritCycleReservationViews(List<SeasonalMeritCycleReservation> items)
    {
        var views = new List<SeasonalMeritCycleReservationView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalMeritCycleReservationView(item));
        }
        return views;
    }

    public static double SumSeasonalMeritCycleReservationView(SeasonalMeritCycleReservationView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedCeiling + (double)parsedLeaveDays;
    }
}
