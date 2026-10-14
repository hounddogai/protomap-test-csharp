namespace Acme.Workforce.Absence.Tests.LeaveRequest.Plan;

using Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class GlobalLeaveRequestBudgetViewChecks
{
    public static void GlobalLeaveRequestBudgetViewShowsTheStatusInCapitals()
    {
        var item = GlobalLeaveRequestBudget.NewGlobalLeaveRequestBudget("code-907", "name-469");
        var view = GlobalLeaveRequestBudgetView.ToGlobalLeaveRequestBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalLeaveRequestBudgetView.ToGlobalLeaveRequestBudgetViews(new List<GlobalLeaveRequestBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalLeaveRequestBudgetViewShowsTheStatusInCapitals();
    }
}
