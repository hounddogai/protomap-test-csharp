namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class PrimaryScenarioSegmentReportChecks
{
    public static void PrimaryScenarioSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700365963);
        var items = new List<PrimaryScenarioSegment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryScenarioSegment.NewPrimaryScenarioSegment($"primary-scenario-segment-{index}", "sample"));
        }
        var report = PrimaryScenarioSegmentReport.BuildPrimaryScenarioSegmentReport("test", items, clock);
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
        PrimaryScenarioSegmentReportCountsEachRecordOnce();
    }
}
