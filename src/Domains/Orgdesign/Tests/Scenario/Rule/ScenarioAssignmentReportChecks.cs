namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScenarioAssignmentReportChecks
{
    public static void ScenarioAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702004531);
        var items = new List<ScenarioAssignment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScenarioAssignment.NewScenarioAssignment($"scenario-assignment-{index}", "sample"));
        }
        var report = ScenarioAssignmentReport.BuildScenarioAssignmentReport("test", items, clock);
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
        ScenarioAssignmentReportCountsEachRecordOnce();
    }
}
