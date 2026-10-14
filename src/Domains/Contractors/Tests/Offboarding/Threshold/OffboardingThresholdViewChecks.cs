namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingThresholdViewChecks
{
    public static void OffboardingThresholdViewShowsTheStatusInCapitals()
    {
        var item = OffboardingThreshold.NewOffboardingThreshold("code-193", "title-959");
        var view = OffboardingThresholdView.ToOffboardingThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingThresholdView.ToOffboardingThresholdViews(new List<OffboardingThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingThresholdViewShowsTheStatusInCapitals();
    }
}
