namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending return plan rollup.</summary>
public sealed record PendingReturnPlanRollupView(string Code, string Title, string CapacityText, string CostText, string Quantity, string Weight, string MinimumText, string FloorValueText, string StatusLabel)
{
    public static PendingReturnPlanRollupView ToPendingReturnPlanRollupView(PendingReturnPlanRollup item)
    {
        return new PendingReturnPlanRollupView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingReturnPlanRollupView> ToPendingReturnPlanRollupViews(List<PendingReturnPlanRollup> items)
    {
        var views = new List<PendingReturnPlanRollupView>();
        foreach (var item in items)
        {
            views.Add(ToPendingReturnPlanRollupView(item));
        }
        return views;
    }

    public static double SumPendingReturnPlanRollupView(PendingReturnPlanRollupView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedCost + (double)parsedQuantity;
    }
}
