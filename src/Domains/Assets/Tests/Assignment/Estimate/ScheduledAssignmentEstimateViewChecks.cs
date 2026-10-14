namespace Acme.Workforce.Assets.Tests.Assignment.Estimate;

using Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ScheduledAssignmentEstimateViewChecks
{
    public static void ScheduledAssignmentEstimateViewShowsTheStatusInCapitals()
    {
        var item = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate("code-625", "label-906");
        var view = ScheduledAssignmentEstimateView.ToScheduledAssignmentEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledAssignmentEstimateView.ToScheduledAssignmentEstimateViews(new List<ScheduledAssignmentEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledAssignmentEstimateViewShowsTheStatusInCapitals();
    }
}
