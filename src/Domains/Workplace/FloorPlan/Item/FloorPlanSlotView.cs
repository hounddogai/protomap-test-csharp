namespace Acme.Workforce.Workplace.FloorPlan.Item;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan slot.</summary>
public sealed record FloorPlanSlotView(string CodeText, string Label, string EmployeeId, string EndsAtText, string Currency, string Score, string Budget, string Grade, string StatusLabel)
{
    public static FloorPlanSlotView ToFloorPlanSlotView(FloorPlanSlot item)
    {
        return new FloorPlanSlotView(
            item.Code,
            item.Label,
            Text.Truncate(item.EmployeeId, 64),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanSlotView> ToFloorPlanSlotViews(List<FloorPlanSlot> items)
    {
        var views = new List<FloorPlanSlotView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanSlotView(item));
        }
        return views;
    }

    public static double SumFloorPlanSlotView(FloorPlanSlotView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedScore + parsedBudget;
    }
}
