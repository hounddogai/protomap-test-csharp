namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class ScenarioTargetReportChecks
{
    public static void ScenarioTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705507085);
        var items = new List<ScenarioTarget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ScenarioTarget.NewScenarioTarget($"scenario-target-{index}", "sample"));
        }
        var report = ScenarioTargetReport.BuildScenarioTargetReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScenarioTargetReportCountsEachRecordOnce();
    }
}
