namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshAssignmentViewChecks
{
    public static void RefreshAssignmentViewShowsTheStatusInCapitals()
    {
        var item = RefreshAssignment.NewRefreshAssignment("code-481", "label-518");
        var view = RefreshAssignmentView.ToRefreshAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshAssignmentView.ToRefreshAssignmentViews(new List<RefreshAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshAssignmentViewShowsTheStatusInCapitals();
    }
}
