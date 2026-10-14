namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DefaultPolicyAssignmentViewChecks
{
    public static void DefaultPolicyAssignmentViewShowsTheStatusInCapitals()
    {
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("code-296", "name-825");
        var view = DefaultPolicyAssignmentView.ToDefaultPolicyAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPolicyAssignmentView.ToDefaultPolicyAssignmentViews(new List<DefaultPolicyAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPolicyAssignmentViewShowsTheStatusInCapitals();
    }
}
