namespace Acme.Workforce.Payrollops.Deduction.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction cycle.</summary>
public sealed record DeductionCycleView(string Code, string NameText, string EndsAt, string DepartmentText, string Region, string BatchSize, string SegmentText, string ManagerId, string Label, string SkillText, string StatusLabel)
{
    public static DeductionCycleView ToDeductionCycleView(DeductionCycle item)
    {
        return new DeductionCycleView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 64),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 40),
            Text.Truncate(item.Region, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.ManagerId,
            Text.Truncate(item.Label, 64),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionCycleView> ToDeductionCycleViews(List<DeductionCycle> items)
    {
        var views = new List<DeductionCycleView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionCycleView(item));
        }
        return views;
    }

    public static double SumDeductionCycleView(DeductionCycleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedBatchSize;
    }
}
