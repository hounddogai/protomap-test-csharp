namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class ManualScenarioAssignmentReportChecks
{
    public static void ManualScenarioAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703442089);
        var items = new List<ManualScenarioAssignment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ManualScenarioAssignment.NewManualScenarioAssignment($"manual-scenario-assignment-{index}", "sample"));
        }
        var report = ManualScenarioAssignmentReport.BuildManualScenarioAssignmentReport("test", items, clock);
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
        ManualScenarioAssignmentReportCountsEachRecordOnce();
    }
}
