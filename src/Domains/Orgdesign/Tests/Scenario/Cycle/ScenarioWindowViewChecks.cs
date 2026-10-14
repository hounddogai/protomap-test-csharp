namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioWindowViewChecks
{
    public static void ScenarioWindowViewShowsTheStatusInCapitals()
    {
        var item = ScenarioWindow.NewScenarioWindow("code-764", "label-111");
        var view = ScenarioWindowView.ToScenarioWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioWindowView.ToScenarioWindowViews(new List<ScenarioWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioWindowViewShowsTheStatusInCapitals();
    }
}
