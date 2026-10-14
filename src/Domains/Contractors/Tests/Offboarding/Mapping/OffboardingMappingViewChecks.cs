namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class OffboardingMappingViewChecks
{
    public static void OffboardingMappingViewShowsTheStatusInCapitals()
    {
        var item = OffboardingMapping.NewOffboardingMapping("code-647", "title-205");
        var view = OffboardingMappingView.ToOffboardingMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingMappingView.ToOffboardingMappingViews(new List<OffboardingMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingMappingViewShowsTheStatusInCapitals();
    }
}
