namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class ManualScenarioWindowReportChecks
{
    public static void ManualScenarioWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703478219);
        var items = new List<ManualScenarioWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ManualScenarioWindow.NewManualScenarioWindow($"manual-scenario-window-{index}", "sample"));
        }
        var report = ManualScenarioWindowReport.BuildManualScenarioWindowReport("test", items, clock);
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
        ManualScenarioWindowReportCountsEachRecordOnce();
    }
}
