namespace Acme.Workforce.Assets.Tests.Assignment.Profile;

using Acme.Workforce.Assets.Assignment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssignmentProfileViewChecks
{
    public static void AssignmentProfileViewShowsTheStatusInCapitals()
    {
        var item = AssignmentProfile.NewAssignmentProfile("code-542", "name-81");
        var view = AssignmentProfileView.ToAssignmentProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentProfileView.ToAssignmentProfileViews(new List<AssignmentProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentProfileViewShowsTheStatusInCapitals();
    }
}
