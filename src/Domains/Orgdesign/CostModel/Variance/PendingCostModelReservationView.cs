namespace Acme.Workforce.Orgdesign.CostModel.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending cost model reservation.</summary>
public sealed record PendingCostModelReservationView(string Code, string Name, string TenureMonths, string DurationMinutes, string VisibleText, string Version, string FloorValue, string Minimum, string Country, string Priority, string StatusLabel)
{
    public static PendingCostModelReservationView ToPendingCostModelReservationView(PendingCostModelReservation item)
    {
        return new PendingCostModelReservationView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingCostModelReservationView> ToPendingCostModelReservationViews(List<PendingCostModelReservation> items)
    {
        var views = new List<PendingCostModelReservationView>();
        foreach (var item in items)
        {
            views.Add(ToPendingCostModelReservationView(item));
        }
        return views;
    }

    public static double SumPendingCostModelReservationView(PendingCostModelReservationView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedDurationMinutes + (double)parsedVersion;
    }
}
