namespace Acme.Workforce.Orgdesign.Tests.Scenario.Metric;

using Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScenarioMetricViewChecks
{
    public static void ScenarioMetricViewShowsTheStatusInCapitals()
    {
        var item = ScenarioMetric.NewScenarioMetric("code-28", "title-60");
        var view = ScenarioMetricView.ToScenarioMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioMetricView.ToScenarioMetricViews(new List<ScenarioMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioMetricViewShowsTheStatusInCapitals();
    }
}
