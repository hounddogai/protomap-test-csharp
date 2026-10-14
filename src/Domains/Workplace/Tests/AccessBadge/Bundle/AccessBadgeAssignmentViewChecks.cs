namespace Acme.Workforce.Workplace.Tests.AccessBadge.Bundle;

using Acme.Workforce.Workplace.AccessBadge.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AccessBadgeAssignmentViewChecks
{
    public static void AccessBadgeAssignmentViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeAssignment.NewAccessBadgeAssignment("code-821", "name-904");
        var view = AccessBadgeAssignmentView.ToAccessBadgeAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeAssignmentView.ToAccessBadgeAssignmentViews(new List<AccessBadgeAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeAssignmentViewShowsTheStatusInCapitals();
    }
}
