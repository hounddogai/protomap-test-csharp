namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AssignmentTargetViewChecks
{
    public static void AssignmentTargetViewShowsTheStatusInCapitals()
    {
        var item = AssignmentTarget.NewAssignmentTarget("code-575", "label-783");
        var view = AssignmentTargetView.ToAssignmentTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentTargetView.ToAssignmentTargetViews(new List<AssignmentTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentTargetViewShowsTheStatusInCapitals();
    }
}
