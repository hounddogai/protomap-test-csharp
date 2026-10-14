namespace Acme.Workforce.Orgdesign.Tests.Scenario.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScenarioBaselineReportChecks
{
    public static void ScenarioBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703474079);
        var items = new List<ScenarioBaseline>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ScenarioBaseline.NewScenarioBaseline($"scenario-baseline-{index}", "sample"));
        }
        var report = ScenarioBaselineReport.BuildScenarioBaselineReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScenarioBaselineReportCountsEachRecordOnce();
    }
}
