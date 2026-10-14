namespace Acme.Workforce.Workplace.Tests.Booking.BookingPlan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.BookingPlan;

/// <summary>Checks of booking plan code, run by the project's runner.</summary>
public static class ManualBookingAuditReportChecks
{
    public static void ManualBookingAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704392713);
        var items = new List<ManualBookingAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ManualBookingAudit.NewManualBookingAudit($"manual-booking-audit-{index}", "sample"));
        }
        var report = ManualBookingAuditReport.BuildManualBookingAuditReport("test", items, clock);
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
        ManualBookingAuditReportCountsEachRecordOnce();
    }
}
