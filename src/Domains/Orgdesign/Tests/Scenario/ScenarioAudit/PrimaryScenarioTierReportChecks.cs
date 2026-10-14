namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class PrimaryScenarioTierReportChecks
{
    public static void PrimaryScenarioTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702835127);
        var items = new List<PrimaryScenarioTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PrimaryScenarioTier.NewPrimaryScenarioTier($"primary-scenario-tier-{index}", "sample"));
        }
        var report = PrimaryScenarioTierReport.BuildPrimaryScenarioTierReport("test", items, clock);
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
        PrimaryScenarioTierReportCountsEachRecordOnce();
    }
}
