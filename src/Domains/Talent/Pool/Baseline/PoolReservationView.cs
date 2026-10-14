namespace Acme.Workforce.Talent.Pool.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool reservation.</summary>
public sealed record PoolReservationView(string Code, string Label, string Capacity, string Score, string FloorValue, string StatusLabel)
{
    public static PoolReservationView ToPoolReservationView(PoolReservation item)
    {
        return new PoolReservationView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 40),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolReservationView> ToPoolReservationViews(List<PoolReservation> items)
    {
        var views = new List<PoolReservationView>();
        foreach (var item in items)
        {
            views.Add(ToPoolReservationView(item));
        }
        return views;
    }

    public static double SumPoolReservationView(PoolReservationView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedScore + parsedFloorValue;
    }
}
