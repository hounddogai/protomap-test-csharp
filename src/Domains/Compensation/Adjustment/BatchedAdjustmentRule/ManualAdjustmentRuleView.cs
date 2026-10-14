namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual adjustment rule.</summary>
public sealed record ManualAdjustmentRuleView(string Code, string Title, string Department, string FloorValueText, string EmployeeId, string SalaryBandText, string Ceiling, string Capacity, string StatusLabel)
{
    public static ManualAdjustmentRuleView ToManualAdjustmentRuleView(ManualAdjustmentRule item)
    {
        return new ManualAdjustmentRuleView(
            item.Code,
            item.Title,
            Text.Truncate(item.Department, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 80),
            item.SalaryBand,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualAdjustmentRuleView> ToManualAdjustmentRuleViews(List<ManualAdjustmentRule> items)
    {
        var views = new List<ManualAdjustmentRuleView>();
        foreach (var item in items)
        {
            views.Add(ToManualAdjustmentRuleView(item));
        }
        return views;
    }

    public static double SumManualAdjustmentRuleView(ManualAdjustmentRuleView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedCeiling + (double)parsedCapacity;
    }
}
