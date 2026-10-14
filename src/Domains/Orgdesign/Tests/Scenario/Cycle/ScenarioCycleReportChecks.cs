namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioCycleReportChecks
{
    public static void ScenarioCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703685732);
        var items = new List<ScenarioCycle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScenarioCycle.NewScenarioCycle($"scenario-cycle-{index}", "sample"));
        }
        var report = ScenarioCycleReport.BuildScenarioCycleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScenarioCycleReportCountsEachRecordOnce();
    }
}
