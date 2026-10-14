namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingRequestViewChecks
{
    public static void OffboardingRequestViewShowsTheStatusInCapitals()
    {
        var item = OffboardingRequest.NewOffboardingRequest("code-544", "title-462");
        var view = OffboardingRequestView.ToOffboardingRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingRequestView.ToOffboardingRequestViews(new List<OffboardingRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingRequestViewShowsTheStatusInCapitals();
    }
}
