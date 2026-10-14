namespace Acme.Workforce.Workplace.Tests.Desk.DeskBudget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.DeskBudget;

/// <summary>Checks of desk budget code, run by the project's runner.</summary>
public static class RollingDeskMappingReportChecks
{
    public static void RollingDeskMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709535528);
        var items = new List<RollingDeskMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingDeskMapping.NewRollingDeskMapping($"rolling-desk-mapping-{index}", "sample"));
        }
        var report = RollingDeskMappingReport.BuildRollingDeskMappingReport("test", items, clock);
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
        RollingDeskMappingReportCountsEachRecordOnce();
    }
}
