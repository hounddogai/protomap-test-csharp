namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class RollingRetroPayAssignmentViewChecks
{
    public static void RollingRetroPayAssignmentViewShowsTheStatusInCapitals()
    {
        var item = RollingRetroPayAssignment.NewRollingRetroPayAssignment("code-80", "title-544");
        var view = RollingRetroPayAssignmentView.ToRollingRetroPayAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingRetroPayAssignmentView.ToRollingRetroPayAssignmentViews(new List<RollingRetroPayAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingRetroPayAssignmentViewShowsTheStatusInCapitals();
    }
}
