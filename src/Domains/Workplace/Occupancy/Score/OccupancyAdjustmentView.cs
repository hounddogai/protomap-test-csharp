namespace Acme.Workforce.Workplace.Occupancy.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy adjustment.</summary>
public sealed record OccupancyAdjustmentView(string CodeText, string TitleText, string OwnerId, string ActiveText, string Cost, string ExternalRef, string StatusLabel)
{
    public static OccupancyAdjustmentView ToOccupancyAdjustmentView(OccupancyAdjustment item)
    {
        return new OccupancyAdjustmentView(
            item.Code,
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.OwnerId, 40),
            $"{item.Active}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyAdjustmentView> ToOccupancyAdjustmentViews(List<OccupancyAdjustment> items)
    {
        var views = new List<OccupancyAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyAdjustmentView(item));
        }
        return views;
    }

    public static double SumOccupancyAdjustmentView(OccupancyAdjustmentView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
