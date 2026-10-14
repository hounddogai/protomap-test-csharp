namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredMobilityAuditReportChecks
{
    public static void TieredMobilityAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706812591);
        var items = new List<TieredMobilityAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredMobilityAudit.NewTieredMobilityAudit($"tiered-mobility-audit-{index}", "sample"));
        }
        var report = TieredMobilityAuditReport.BuildTieredMobilityAuditReport("test", items, clock);
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
        TieredMobilityAuditReportCountsEachRecordOnce();
    }
}
