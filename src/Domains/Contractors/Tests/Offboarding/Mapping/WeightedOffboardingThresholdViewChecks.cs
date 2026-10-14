namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class WeightedOffboardingThresholdViewChecks
{
    public static void WeightedOffboardingThresholdViewShowsTheStatusInCapitals()
    {
        var item = WeightedOffboardingThreshold.NewWeightedOffboardingThreshold("code-330", "label-967");
        var view = WeightedOffboardingThresholdView.ToWeightedOffboardingThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedOffboardingThresholdView.ToWeightedOffboardingThresholdViews(new List<WeightedOffboardingThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedOffboardingThresholdViewShowsTheStatusInCapitals();
    }
}
