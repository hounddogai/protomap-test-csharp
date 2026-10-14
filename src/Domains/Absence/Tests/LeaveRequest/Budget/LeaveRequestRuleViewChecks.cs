namespace Acme.Workforce.Absence.Tests.LeaveRequest.Budget;

using Acme.Workforce.Absence.LeaveRequest.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LeaveRequestRuleViewChecks
{
    public static void LeaveRequestRuleViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestRule.NewLeaveRequestRule("code-782", "label-521");
        var view = LeaveRequestRuleView.ToLeaveRequestRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestRuleView.ToLeaveRequestRuleViews(new List<LeaveRequestRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestRuleViewShowsTheStatusInCapitals();
    }
}
