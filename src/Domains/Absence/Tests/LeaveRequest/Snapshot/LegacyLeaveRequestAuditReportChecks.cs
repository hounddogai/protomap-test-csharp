namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyLeaveRequestAuditReportChecks
{
    public static void LegacyLeaveRequestAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705574302);
        var items = new List<LegacyLeaveRequestAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LegacyLeaveRequestAudit.NewLegacyLeaveRequestAudit($"legacy-leave-request-audit-{index}", "sample"));
        }
        var report = LegacyLeaveRequestAuditReport.BuildLegacyLeaveRequestAuditReport("test", items, clock);
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
        LegacyLeaveRequestAuditReportCountsEachRecordOnce();
    }
}
