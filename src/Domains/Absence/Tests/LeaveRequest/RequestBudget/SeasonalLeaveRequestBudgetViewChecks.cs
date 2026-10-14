namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestBudget;

using Acme.Workforce.Absence.LeaveRequest.RequestBudget;

/// <summary>Checks of request budget code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestBudgetViewChecks
{
    public static void SeasonalLeaveRequestBudgetViewShowsTheStatusInCapitals()
    {
        var item = SeasonalLeaveRequestBudget.NewSeasonalLeaveRequestBudget("code-880", "name-237");
        var view = SeasonalLeaveRequestBudgetView.ToSeasonalLeaveRequestBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalLeaveRequestBudgetView.ToSeasonalLeaveRequestBudgetViews(new List<SeasonalLeaveRequestBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalLeaveRequestBudgetViewShowsTheStatusInCapitals();
    }
}
