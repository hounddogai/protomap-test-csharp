namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ManualScenarioEstimateReportChecks
{
    public static void ManualScenarioEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706870932);
        var items = new List<ManualScenarioEstimate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualScenarioEstimate.NewManualScenarioEstimate($"manual-scenario-estimate-{index}", "sample"));
        }
        var report = ManualScenarioEstimateReport.BuildManualScenarioEstimateReport("test", items, clock);
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
        ManualScenarioEstimateReportCountsEachRecordOnce();
    }
}
