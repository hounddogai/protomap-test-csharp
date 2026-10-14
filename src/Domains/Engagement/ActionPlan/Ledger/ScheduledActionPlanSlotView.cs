namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled action plan slot.</summary>
public sealed record ScheduledActionPlanSlotView(string CodeText, string Label, string DepartmentText, string ApprovedText, string TenureMonthsText, string Weight, string ManagerId, string LocaleText, string Threshold, string StatusLabel)
{
    public static ScheduledActionPlanSlotView ToScheduledActionPlanSlotView(ScheduledActionPlanSlot item)
    {
        return new ScheduledActionPlanSlotView(
            item.Code,
            item.Label,
            Text.Truncate(item.Department, 120),
            $"{item.Approved}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 80),
            item.Locale,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledActionPlanSlotView> ToScheduledActionPlanSlotViews(List<ScheduledActionPlanSlot> items)
    {
        var views = new List<ScheduledActionPlanSlotView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledActionPlanSlotView(item));
        }
        return views;
    }

    public static double SumScheduledActionPlanSlotView(ScheduledActionPlanSlotView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedWeight + parsedThreshold;
    }
}
