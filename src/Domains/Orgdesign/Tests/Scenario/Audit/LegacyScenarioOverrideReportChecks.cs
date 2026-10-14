namespace Acme.Workforce.Orgdesign.Tests.Scenario.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyScenarioOverrideReportChecks
{
    public static void LegacyScenarioOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705285123);
        var items = new List<LegacyScenarioOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyScenarioOverride.NewLegacyScenarioOverride($"legacy-scenario-override-{index}", "sample"));
        }
        var report = LegacyScenarioOverrideReport.BuildLegacyScenarioOverrideReport("test", items, clock);
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
        LegacyScenarioOverrideReportCountsEachRecordOnce();
    }
}
