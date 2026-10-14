namespace Acme.Workforce.Compensation.MeritCycle.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle reservation.</summary>
public sealed record MeritCycleReservationView(string Code, string Label, string LocaleText, string Source, string Category, string Retries, string Budget, string StatusLabel)
{
    public static MeritCycleReservationView ToMeritCycleReservationView(MeritCycleReservation item)
    {
        return new MeritCycleReservationView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Locale,
            item.Source,
            Text.Truncate(item.Category, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleReservationView> ToMeritCycleReservationViews(List<MeritCycleReservation> items)
    {
        var views = new List<MeritCycleReservationView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleReservationView(item));
        }
        return views;
    }

    public static double SumMeritCycleReservationView(MeritCycleReservationView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedBudget;
    }
}
