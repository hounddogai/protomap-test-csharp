namespace Acme.Workforce.Assets.Tests.Assignment.Agreement;

using Acme.Workforce.Assets.Assignment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssignmentPolicyViewChecks
{
    public static void AssignmentPolicyViewShowsTheStatusInCapitals()
    {
        var item = AssignmentPolicy.NewAssignmentPolicy("code-151", "title-788");
        var view = AssignmentPolicyView.ToAssignmentPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentPolicyView.ToAssignmentPolicyViews(new List<AssignmentPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentPolicyViewShowsTheStatusInCapitals();
    }
}
