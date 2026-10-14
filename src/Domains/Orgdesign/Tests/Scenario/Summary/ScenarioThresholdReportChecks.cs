namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScenarioThresholdReportChecks
{
    public static void ScenarioThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702686592);
        var items = new List<ScenarioThreshold>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScenarioThreshold.NewScenarioThreshold($"scenario-threshold-{index}", "sample"));
        }
        var report = ScenarioThresholdReport.BuildScenarioThresholdReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScenarioThresholdReportCountsEachRecordOnce();
    }
}
