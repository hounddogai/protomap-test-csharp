namespace Acme.Workforce.Orgdesign.Tests.Scenario.Metric;

using Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScenarioMetricChecks
{
    public static void ScenarioMetricStartsInCreated()
    {
        var item = ScenarioMetric.NewScenarioMetric("code-28", "title-60");
        Check.Equal(item.Status, ScenarioMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioMetricMovesToValidated()
    {
        var item = ScenarioMetric.NewScenarioMetric("code-28", "title-60");
        try
        {
            item.MoveTo(ScenarioMetricStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioMetricStatus.Validated, "status");
    }

    public static void ScenarioMetricRejectsMovesFromRetired()
    {
        var item = ScenarioMetric.NewScenarioMetric("code-28", "title-60");
        item.Status = ScenarioMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(ScenarioMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScenarioMetricStartsInCreated();
        ScenarioMetricMovesToValidated();
        ScenarioMetricRejectsMovesFromRetired();
    }
}
