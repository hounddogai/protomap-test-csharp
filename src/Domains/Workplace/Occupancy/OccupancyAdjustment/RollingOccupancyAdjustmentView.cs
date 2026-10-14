namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling occupancy adjustment.</summary>
public sealed record RollingOccupancyAdjustmentView(string Code, string LabelText, string Version, string Ratio, string Region, string Quantity, string StatusLabel)
{
    public static RollingOccupancyAdjustmentView ToRollingOccupancyAdjustmentView(RollingOccupancyAdjustment item)
    {
        return new RollingOccupancyAdjustmentView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingOccupancyAdjustmentView> ToRollingOccupancyAdjustmentViews(List<RollingOccupancyAdjustment> items)
    {
        var views = new List<RollingOccupancyAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToRollingOccupancyAdjustmentView(item));
        }
        return views;
    }

    public static double SumRollingOccupancyAdjustmentView(RollingOccupancyAdjustmentView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedRatio + (double)parsedQuantity;
    }
}
