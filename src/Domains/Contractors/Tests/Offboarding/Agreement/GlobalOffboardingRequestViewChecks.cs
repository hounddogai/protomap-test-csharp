namespace Acme.Workforce.Contractors.Tests.Offboarding.Agreement;

using Acme.Workforce.Contractors.Offboarding.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class GlobalOffboardingRequestViewChecks
{
    public static void GlobalOffboardingRequestViewShowsTheStatusInCapitals()
    {
        var item = GlobalOffboardingRequest.NewGlobalOffboardingRequest("code-387", "label-213");
        var view = GlobalOffboardingRequestView.ToGlobalOffboardingRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalOffboardingRequestView.ToGlobalOffboardingRequestViews(new List<GlobalOffboardingRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalOffboardingRequestViewShowsTheStatusInCapitals();
    }
}
