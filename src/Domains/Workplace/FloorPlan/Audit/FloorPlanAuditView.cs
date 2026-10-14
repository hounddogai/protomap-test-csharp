namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using System.Globalization;

/// <summary>How screens and exports show a floor plan audit.</summary>
public sealed record FloorPlanAuditView(string Code, string Title, string CurrencyText, string Ceiling, string Region, string Discount, string Visible, string StatusLabel)
{
    public static FloorPlanAuditView ToFloorPlanAuditView(FloorPlanAudit item)
    {
        return new FloorPlanAuditView(
            item.Code,
            item.Title,
            item.Currency,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanAuditView> ToFloorPlanAuditViews(List<FloorPlanAudit> items)
    {
        var views = new List<FloorPlanAuditView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanAuditView(item));
        }
        return views;
    }

    public static double SumFloorPlanAuditView(FloorPlanAuditView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedDiscount;
    }
}
