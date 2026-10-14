namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class CappedScenarioEstimateReportChecks
{
    public static void CappedScenarioEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701010851);
        var items = new List<CappedScenarioEstimate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedScenarioEstimate.NewCappedScenarioEstimate($"capped-scenario-estimate-{index}", "sample"));
        }
        var report = CappedScenarioEstimateReport.BuildCappedScenarioEstimateReport("test", items, clock);
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
        CappedScenarioEstimateReportCountsEachRecordOnce();
    }
}
