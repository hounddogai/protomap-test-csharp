namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ManualAuditScheduleReportChecks
{
    public static void ManualAuditScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704323648);
        var items = new List<ManualAuditSchedule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualAuditSchedule.NewManualAuditSchedule($"manual-audit-schedule-{index}", "sample"));
        }
        var report = ManualAuditScheduleReport.BuildManualAuditScheduleReport("test", items, clock);
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
        ManualAuditScheduleReportCountsEachRecordOnce();
    }
}
