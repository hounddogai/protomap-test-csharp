namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class GlobalScenarioBundleReportChecks
{
    public static void GlobalScenarioBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703616680);
        var items = new List<GlobalScenarioBundle>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(GlobalScenarioBundle.NewGlobalScenarioBundle($"global-scenario-bundle-{index}", "sample"));
        }
        var report = GlobalScenarioBundleReport.BuildGlobalScenarioBundleReport("test", items, clock);
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
        GlobalScenarioBundleReportCountsEachRecordOnce();
    }
}
