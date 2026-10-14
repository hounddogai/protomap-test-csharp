namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling mobility reservation.</summary>
public sealed record RollingMobilityReservationView(string CodeText, string Title, string CreatedAtText, string GraceDays, string EndsAt, string Ceiling, string WeightText, string StatusLabel)
{
    public static RollingMobilityReservationView ToRollingMobilityReservationView(RollingMobilityReservation item)
    {
        return new RollingMobilityReservationView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingMobilityReservationView> ToRollingMobilityReservationViews(List<RollingMobilityReservation> items)
    {
        var views = new List<RollingMobilityReservationView>();
        foreach (var item in items)
        {
            views.Add(ToRollingMobilityReservationView(item));
        }
        return views;
    }

    public static double SumRollingMobilityReservationView(RollingMobilityReservationView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedGraceDays + (double)parsedEndsAt;
    }
}
