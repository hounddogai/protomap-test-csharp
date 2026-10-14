namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using System.Globalization;

/// <summary>How screens and exports show a leave request budget.</summary>
public sealed record LeaveRequestBudgetView(string Code, string Label, string Locale, string PriorityText, string Grade, string Pinned, string StatusLabel)
{
    public static LeaveRequestBudgetView ToLeaveRequestBudgetView(LeaveRequestBudget item)
    {
        return new LeaveRequestBudgetView(
            item.Code,
            item.Label,
            item.Locale,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestBudgetView> ToLeaveRequestBudgetViews(List<LeaveRequestBudget> items)
    {
        var views = new List<LeaveRequestBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestBudgetView(item));
        }
        return views;
    }

    public static double SumLeaveRequestBudgetView(LeaveRequestBudgetView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedPriority;
    }
}
