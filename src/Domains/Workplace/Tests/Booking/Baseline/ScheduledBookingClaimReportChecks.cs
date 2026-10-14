namespace Acme.Workforce.Workplace.Tests.Booking.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledBookingClaimReportChecks
{
    public static void ScheduledBookingClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704865976);
        var items = new List<ScheduledBookingClaim>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledBookingClaim.NewScheduledBookingClaim($"scheduled-booking-claim-{index}", "sample"));
        }
        var report = ScheduledBookingClaimReport.BuildScheduledBookingClaimReport("test", items, clock);
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
        ScheduledBookingClaimReportCountsEachRecordOnce();
    }
}
