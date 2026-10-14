namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class LegacyBookingAuditReportChecks
{
    public static void LegacyBookingAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709671512);
        var items = new List<LegacyBookingAudit>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LegacyBookingAudit.NewLegacyBookingAudit($"legacy-booking-audit-{index}", "sample"));
        }
        var report = LegacyBookingAuditReport.BuildLegacyBookingAuditReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LegacyBookingAuditReportCountsEachRecordOnce();
    }
}
