namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class ScenarioTargetViewChecks
{
    public static void ScenarioTargetViewShowsTheStatusInCapitals()
    {
        var item = ScenarioTarget.NewScenarioTarget("code-924", "name-949");
        var view = ScenarioTargetView.ToScenarioTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioTargetView.ToScenarioTargetViews(new List<ScenarioTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioTargetViewShowsTheStatusInCapitals();
    }
}
