namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a timesheet plan.</summary>
public sealed record TimesheetPlanView(string Code, string Title, string Attempts, string Fte, string Quantity, string NameText, string EmployeeIdText, string OwnerIdText, string StatusLabel)
{
    public static TimesheetPlanView ToTimesheetPlanView(TimesheetPlan item)
    {
        return new TimesheetPlanView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 80),
            item.EmployeeId,
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TimesheetPlanView> ToTimesheetPlanViews(List<TimesheetPlan> items)
    {
        var views = new List<TimesheetPlanView>();
        foreach (var item in items)
        {
            views.Add(ToTimesheetPlanView(item));
        }
        return views;
    }

    public static double SumTimesheetPlanView(TimesheetPlanView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedFte + (double)parsedQuantity;
    }
}
