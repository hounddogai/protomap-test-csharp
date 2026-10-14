namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestBudgetViewChecks
{
    public static void PrimaryLeaveRequestBudgetViewShowsTheStatusInCapitals()
    {
        var item = PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget("code-112", "title-819");
        var view = PrimaryLeaveRequestBudgetView.ToPrimaryLeaveRequestBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryLeaveRequestBudgetView.ToPrimaryLeaveRequestBudgetViews(new List<PrimaryLeaveRequestBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryLeaveRequestBudgetViewShowsTheStatusInCapitals();
    }
}
