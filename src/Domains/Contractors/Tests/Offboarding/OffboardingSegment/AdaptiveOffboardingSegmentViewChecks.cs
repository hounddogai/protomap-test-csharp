namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

/// <summary>Checks of offboarding segment code, run by the project's runner.</summary>
public static class AdaptiveOffboardingSegmentViewChecks
{
    public static void AdaptiveOffboardingSegmentViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveOffboardingSegment.NewAdaptiveOffboardingSegment("code-108", "title-309");
        var view = AdaptiveOffboardingSegmentView.ToAdaptiveOffboardingSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveOffboardingSegmentView.ToAdaptiveOffboardingSegmentViews(new List<AdaptiveOffboardingSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveOffboardingSegmentViewShowsTheStatusInCapitals();
    }
}
