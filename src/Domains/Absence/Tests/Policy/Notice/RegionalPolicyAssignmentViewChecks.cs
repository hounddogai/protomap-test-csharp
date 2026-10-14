namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RegionalPolicyAssignmentViewChecks
{
    public static void RegionalPolicyAssignmentViewShowsTheStatusInCapitals()
    {
        var item = RegionalPolicyAssignment.NewRegionalPolicyAssignment("code-279", "title-497");
        var view = RegionalPolicyAssignmentView.ToRegionalPolicyAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalPolicyAssignmentView.ToRegionalPolicyAssignmentViews(new List<RegionalPolicyAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalPolicyAssignmentViewShowsTheStatusInCapitals();
    }
}
