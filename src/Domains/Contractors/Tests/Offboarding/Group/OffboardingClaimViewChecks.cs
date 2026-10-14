namespace Acme.Workforce.Contractors.Tests.Offboarding.Group;

using Acme.Workforce.Contractors.Offboarding.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class OffboardingClaimViewChecks
{
    public static void OffboardingClaimViewShowsTheStatusInCapitals()
    {
        var item = OffboardingClaim.NewOffboardingClaim("code-556", "name-788");
        var view = OffboardingClaimView.ToOffboardingClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingClaimView.ToOffboardingClaimViews(new List<OffboardingClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingClaimViewShowsTheStatusInCapitals();
    }
}
