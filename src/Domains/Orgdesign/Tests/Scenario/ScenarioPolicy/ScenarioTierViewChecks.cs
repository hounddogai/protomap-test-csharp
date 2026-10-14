namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioTierViewChecks
{
    public static void ScenarioTierViewShowsTheStatusInCapitals()
    {
        var item = ScenarioTier.NewScenarioTier("code-298", "name-825");
        var view = ScenarioTierView.ToScenarioTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioTierView.ToScenarioTierViews(new List<ScenarioTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioTierViewShowsTheStatusInCapitals();
    }
}
