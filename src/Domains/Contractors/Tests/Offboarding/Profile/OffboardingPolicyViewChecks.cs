namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingPolicyViewChecks
{
    public static void OffboardingPolicyViewShowsTheStatusInCapitals()
    {
        var item = OffboardingPolicy.NewOffboardingPolicy("code-331", "title-199");
        var view = OffboardingPolicyView.ToOffboardingPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingPolicyView.ToOffboardingPolicyViews(new List<OffboardingPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingPolicyViewShowsTheStatusInCapitals();
    }
}
