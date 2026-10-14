namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScenarioThresholdViewChecks
{
    public static void ScenarioThresholdViewShowsTheStatusInCapitals()
    {
        var item = ScenarioThreshold.NewScenarioThreshold("code-922", "label-534");
        var view = ScenarioThresholdView.ToScenarioThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioThresholdView.ToScenarioThresholdViews(new List<ScenarioThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioThresholdViewShowsTheStatusInCapitals();
    }
}
