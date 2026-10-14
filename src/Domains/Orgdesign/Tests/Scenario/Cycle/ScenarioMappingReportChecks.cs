namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioMappingReportChecks
{
    public static void ScenarioMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700205393);
        var items = new List<ScenarioMapping>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ScenarioMapping.NewScenarioMapping($"scenario-mapping-{index}", "sample"));
        }
        var report = ScenarioMappingReport.BuildScenarioMappingReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScenarioMappingReportCountsEachRecordOnce();
    }
}
