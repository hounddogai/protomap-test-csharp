namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional deduction schedule.</summary>
public sealed record RegionalDeductionScheduleView(string Code, string Label, string FloorValue, string Department, string UpdatedAtText, string ExternalRef, string ThresholdText, string ManagerId, string Name, string Cost, string StatusLabel)
{
    public static RegionalDeductionScheduleView ToRegionalDeductionScheduleView(RegionalDeductionSchedule item)
    {
        return new RegionalDeductionScheduleView(
            item.Code,
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 40),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 64),
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalDeductionScheduleView> ToRegionalDeductionScheduleViews(List<RegionalDeductionSchedule> items)
    {
        var views = new List<RegionalDeductionScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalDeductionScheduleView(item));
        }
        return views;
    }

    public static double SumRegionalDeductionScheduleView(RegionalDeductionScheduleView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedUpdatedAt + parsedThreshold;
    }
}
