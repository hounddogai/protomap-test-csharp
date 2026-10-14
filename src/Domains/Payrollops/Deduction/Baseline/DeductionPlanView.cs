namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a deduction plan.</summary>
public sealed record DeductionPlanView(string CodeText, string Label, string PinnedText, string Ceiling, string Segment, string WindowDays, string DurationMinutes, string DepartmentText, string StartsAtText, string LeaveDays, string StatusLabel)
{
    public static DeductionPlanView ToDeductionPlanView(DeductionPlan item)
    {
        return new DeductionPlanView(
            item.Code,
            item.Label,
            $"{item.Pinned}",
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionPlanView> ToDeductionPlanViews(List<DeductionPlan> items)
    {
        var views = new List<DeductionPlanView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionPlanView(item));
        }
        return views;
    }

    public static double SumDeductionPlanView(DeductionPlanView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedWindowDays + (double)parsedDurationMinutes;
    }
}
