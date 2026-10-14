namespace Acme.Workforce.Absence.Holiday.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday assignment.</summary>
public sealed record HolidayAssignmentView(string Code, string Name, string BudgetText, string Skill, string Timezone, string ExternalRef, string Team, string EmployeeId, string StatusLabel)
{
    public static HolidayAssignmentView ToHolidayAssignmentView(HolidayAssignment item)
    {
        return new HolidayAssignmentView(
            item.Code,
            item.Name,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.Timezone, 80),
            item.ExternalRef,
            Text.Truncate(item.Team, 40),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayAssignmentView> ToHolidayAssignmentViews(List<HolidayAssignment> items)
    {
        var views = new List<HolidayAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayAssignmentView(item));
        }
        return views;
    }

    public static double SumHolidayAssignmentView(HolidayAssignmentView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
