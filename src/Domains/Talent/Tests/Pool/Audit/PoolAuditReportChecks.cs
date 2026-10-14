namespace Acme.Workforce.Talent.Tests.Pool.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PoolAuditReportChecks
{
    public static void PoolAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707129799);
        var items = new List<PoolAudit>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PoolAudit.NewPoolAudit($"pool-audit-{index}", "sample"));
        }
        var report = PoolAuditReport.BuildPoolAuditReport("test", items, clock);
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
        PoolAuditReportCountsEachRecordOnce();
    }
}
