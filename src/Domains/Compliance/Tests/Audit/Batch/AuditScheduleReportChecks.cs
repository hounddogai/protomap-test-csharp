namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AuditScheduleReportChecks
{
    public static void AuditScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702355950);
        var items = new List<AuditSchedule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AuditSchedule.NewAuditSchedule($"audit-schedule-{index}", "sample"));
        }
        var report = AuditScheduleReport.BuildAuditScheduleReport("test", items, clock);
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
        AuditScheduleReportCountsEachRecordOnce();
    }
}
