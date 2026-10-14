namespace Acme.Workforce.Contractors.Tests.Rate.RateReview;

using Acme.Workforce.Contractors.Rate.RateReview;

/// <summary>Checks of rate review code, run by the project's runner.</summary>
public static class RollingRateAssignmentViewChecks
{
    public static void RollingRateAssignmentViewShowsTheStatusInCapitals()
    {
        var item = RollingRateAssignment.NewRollingRateAssignment("code-204", "label-291");
        var view = RollingRateAssignmentView.ToRollingRateAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingRateAssignmentView.ToRollingRateAssignmentViews(new List<RollingRateAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingRateAssignmentViewShowsTheStatusInCapitals();
    }
}
