namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioCycleViewChecks
{
    public static void ScenarioCycleViewShowsTheStatusInCapitals()
    {
        var item = ScenarioCycle.NewScenarioCycle("code-763", "label-671");
        var view = ScenarioCycleView.ToScenarioCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioCycleView.ToScenarioCycleViews(new List<ScenarioCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioCycleViewShowsTheStatusInCapitals();
    }
}
