namespace Acme.Workforce.Workplace.FloorPlan.Item;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual floor plan item.</summary>
public sealed record ManualFloorPlanItemView(string Code, string Title, string Archived, string LeaveDays, string Hours, string OwnerIdText, string StatusLabel)
{
    public static ManualFloorPlanItemView ToManualFloorPlanItemView(ManualFloorPlanItem item)
    {
        return new ManualFloorPlanItemView(
            item.Code,
            Text.Truncate(item.Title, 120),
            $"{item.Archived}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualFloorPlanItemView> ToManualFloorPlanItemViews(List<ManualFloorPlanItem> items)
    {
        var views = new List<ManualFloorPlanItemView>();
        foreach (var item in items)
        {
            views.Add(ToManualFloorPlanItemView(item));
        }
        return views;
    }

    public static double SumManualFloorPlanItemView(ManualFloorPlanItemView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedHours;
    }
}
