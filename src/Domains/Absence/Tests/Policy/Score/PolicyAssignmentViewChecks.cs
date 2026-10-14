namespace Acme.Workforce.Absence.Tests.Policy.Score;

using Acme.Workforce.Absence.Policy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class PolicyAssignmentViewChecks
{
    public static void PolicyAssignmentViewShowsTheStatusInCapitals()
    {
        var item = PolicyAssignment.NewPolicyAssignment("code-258", "title-612");
        var view = PolicyAssignmentView.ToPolicyAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAssignmentView.ToPolicyAssignmentViews(new List<PolicyAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAssignmentViewShowsTheStatusInCapitals();
    }
}
