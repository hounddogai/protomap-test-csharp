namespace Acme.Workforce.Assets.Tests.Assignment.Rule;

using Acme.Workforce.Assets.Assignment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssignmentEstimateViewChecks
{
    public static void AssignmentEstimateViewShowsTheStatusInCapitals()
    {
        var item = AssignmentEstimate.NewAssignmentEstimate("code-809", "name-62");
        var view = AssignmentEstimateView.ToAssignmentEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentEstimateView.ToAssignmentEstimateViews(new List<AssignmentEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentEstimateViewShowsTheStatusInCapitals();
    }
}
