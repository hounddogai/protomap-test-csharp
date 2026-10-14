namespace Acme.Workforce.Orgdesign.Tests.Scenario.Tier;

using Acme.Workforce.Orgdesign.Scenario.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScenarioBaselineViewChecks
{
    public static void ScenarioBaselineViewShowsTheStatusInCapitals()
    {
        var item = ScenarioBaseline.NewScenarioBaseline("code-318", "label-136");
        var view = ScenarioBaselineView.ToScenarioBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioBaselineView.ToScenarioBaselineViews(new List<ScenarioBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioBaselineViewShowsTheStatusInCapitals();
    }
}
