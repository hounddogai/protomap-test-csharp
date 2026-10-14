namespace Acme.Workforce.Assets.Tests.Assignment.Snapshot;

using Acme.Workforce.Assets.Assignment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class AssignmentOverrideViewChecks
{
    public static void AssignmentOverrideViewShowsTheStatusInCapitals()
    {
        var item = AssignmentOverride.NewAssignmentOverride("code-229", "label-505");
        var view = AssignmentOverrideView.ToAssignmentOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentOverrideView.ToAssignmentOverrideViews(new List<AssignmentOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentOverrideViewShowsTheStatusInCapitals();
    }
}
