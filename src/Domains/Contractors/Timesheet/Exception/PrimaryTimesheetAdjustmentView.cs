namespace Acme.Workforce.Contractors.Timesheet.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary timesheet adjustment.</summary>
public sealed record PrimaryTimesheetAdjustmentView(string CodeText, string Title, string Archived, string Timezone, string DurationMinutes, string CostText, string SalaryBand, string Department, string Channel, string Limit, string Unit, string StatusLabel)
{
    public static PrimaryTimesheetAdjustmentView ToPrimaryTimesheetAdjustmentView(PrimaryTimesheetAdjustment item)
    {
        return new PrimaryTimesheetAdjustmentView(
            item.Code,
            item.Title,
            $"{item.Archived}",
            item.Timezone,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 40),
            item.Department,
            item.Channel,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryTimesheetAdjustmentView> ToPrimaryTimesheetAdjustmentViews(List<PrimaryTimesheetAdjustment> items)
    {
        var views = new List<PrimaryTimesheetAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryTimesheetAdjustmentView(item));
        }
        return views;
    }

    public static double SumPrimaryTimesheetAdjustmentView(PrimaryTimesheetAdjustmentView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedCost + (double)parsedLimit;
    }
}
