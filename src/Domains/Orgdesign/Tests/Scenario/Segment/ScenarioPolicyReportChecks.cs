namespace Acme.Workforce.Orgdesign.Tests.Scenario.Segment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ScenarioPolicyReportChecks
{
    public static void ScenarioPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703299524);
        var items = new List<ScenarioPolicy>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScenarioPolicy.NewScenarioPolicy($"scenario-policy-{index}", "sample"));
        }
        var report = ScenarioPolicyReport.BuildScenarioPolicyReport("test", items, clock);
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
        ScenarioPolicyReportCountsEachRecordOnce();
    }
}
