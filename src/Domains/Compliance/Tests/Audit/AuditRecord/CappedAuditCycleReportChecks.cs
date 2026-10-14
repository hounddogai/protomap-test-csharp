namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class CappedAuditCycleReportChecks
{
    public static void CappedAuditCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706418640);
        var items = new List<CappedAuditCycle>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(CappedAuditCycle.NewCappedAuditCycle($"capped-audit-cycle-{index}", "sample"));
        }
        var report = CappedAuditCycleReport.BuildCappedAuditCycleReport("test", items, clock);
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
        CappedAuditCycleReportCountsEachRecordOnce();
    }
}
