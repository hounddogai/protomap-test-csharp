namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient floor plan audit.</summary>
public sealed record LenientFloorPlanAuditView(string Code, string Name, string QuantityText, string LeaveDays, string BatchSize, string StatusLabel)
{
    public static LenientFloorPlanAuditView ToLenientFloorPlanAuditView(LenientFloorPlanAudit item)
    {
        return new LenientFloorPlanAuditView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientFloorPlanAuditView> ToLenientFloorPlanAuditViews(List<LenientFloorPlanAudit> items)
    {
        var views = new List<LenientFloorPlanAuditView>();
        foreach (var item in items)
        {
            views.Add(ToLenientFloorPlanAuditView(item));
        }
        return views;
    }

    public static double SumLenientFloorPlanAuditView(LenientFloorPlanAuditView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedLeaveDays + (double)parsedBatchSize;
    }
}
