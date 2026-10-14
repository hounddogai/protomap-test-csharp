namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PolicyProfileViewChecks
{
    public static void PolicyProfileViewShowsTheStatusInCapitals()
    {
        var item = PolicyProfile.NewPolicyProfile("code-422", "label-156");
        var view = PolicyProfileView.ToPolicyProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyProfileView.ToPolicyProfileViews(new List<PolicyProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyProfileViewShowsTheStatusInCapitals();
    }
}
