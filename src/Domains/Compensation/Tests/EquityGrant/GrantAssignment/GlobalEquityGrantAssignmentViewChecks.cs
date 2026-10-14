namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantAssignment;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>Checks of grant assignment code, run by the project's runner.</summary>
public static class GlobalEquityGrantAssignmentViewChecks
{
    public static void GlobalEquityGrantAssignmentViewShowsTheStatusInCapitals()
    {
        var item = GlobalEquityGrantAssignment.NewGlobalEquityGrantAssignment("code-875", "label-685");
        var view = GlobalEquityGrantAssignmentView.ToGlobalEquityGrantAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalEquityGrantAssignmentView.ToGlobalEquityGrantAssignmentViews(new List<GlobalEquityGrantAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalEquityGrantAssignmentViewShowsTheStatusInCapitals();
    }
}
