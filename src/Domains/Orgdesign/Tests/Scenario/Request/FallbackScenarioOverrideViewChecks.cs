namespace Acme.Workforce.Orgdesign.Tests.Scenario.Request;

using Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackScenarioOverrideViewChecks
{
    public static void FallbackScenarioOverrideViewShowsTheStatusInCapitals()
    {
        var item = FallbackScenarioOverride.NewFallbackScenarioOverride("code-472", "name-285");
        var view = FallbackScenarioOverrideView.ToFallbackScenarioOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackScenarioOverrideView.ToFallbackScenarioOverrideViews(new List<FallbackScenarioOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackScenarioOverrideViewShowsTheStatusInCapitals();
    }
}
