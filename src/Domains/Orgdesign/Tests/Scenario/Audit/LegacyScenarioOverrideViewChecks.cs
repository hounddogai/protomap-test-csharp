namespace Acme.Workforce.Orgdesign.Tests.Scenario.Audit;

using Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyScenarioOverrideViewChecks
{
    public static void LegacyScenarioOverrideViewShowsTheStatusInCapitals()
    {
        var item = LegacyScenarioOverride.NewLegacyScenarioOverride("code-298", "title-53");
        var view = LegacyScenarioOverrideView.ToLegacyScenarioOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyScenarioOverrideView.ToLegacyScenarioOverrideViews(new List<LegacyScenarioOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyScenarioOverrideViewShowsTheStatusInCapitals();
    }
}
