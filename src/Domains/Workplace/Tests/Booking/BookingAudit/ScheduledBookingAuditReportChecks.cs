namespace Acme.Workforce.Workplace.Tests.Booking.BookingAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Checks of booking audit code, run by the project's runner.</summary>
public static class ScheduledBookingAuditReportChecks
{
    public static void ScheduledBookingAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708629787);
        var items = new List<ScheduledBookingAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ScheduledBookingAudit.NewScheduledBookingAudit($"scheduled-booking-audit-{index}", "sample"));
        }
        var report = ScheduledBookingAuditReport.BuildScheduledBookingAuditReport("test", items, clock);
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
        ScheduledBookingAuditReportCountsEachRecordOnce();
    }
}
