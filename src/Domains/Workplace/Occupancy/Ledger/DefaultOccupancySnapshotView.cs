namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default occupancy snapshot.</summary>
public sealed record DefaultOccupancySnapshotView(string CodeText, string LabelText, string Location, string Summary, string FteText, string CategoryText, string FloorValueText, string LimitText, string Visible, string StatusLabel)
{
    public static DefaultOccupancySnapshotView ToDefaultOccupancySnapshotView(DefaultOccupancySnapshot item)
    {
        return new DefaultOccupancySnapshotView(
            item.Code,
            item.Label,
            item.Location,
            item.Summary,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 120),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultOccupancySnapshotView> ToDefaultOccupancySnapshotViews(List<DefaultOccupancySnapshot> items)
    {
        var views = new List<DefaultOccupancySnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultOccupancySnapshotView(item));
        }
        return views;
    }

    public static double SumDefaultOccupancySnapshotView(DefaultOccupancySnapshotView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return parsedFte + parsedFloorValue + (double)parsedLimit;
    }
}
