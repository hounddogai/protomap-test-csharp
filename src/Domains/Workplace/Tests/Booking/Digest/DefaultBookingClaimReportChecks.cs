namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DefaultBookingClaimReportChecks
{
    public static void DefaultBookingClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705228343);
        var items = new List<DefaultBookingClaim>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultBookingClaim.NewDefaultBookingClaim($"default-booking-claim-{index}", "sample"));
        }
        var report = DefaultBookingClaimReport.BuildDefaultBookingClaimReport("test", items, clock);
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
        DefaultBookingClaimReportCountsEachRecordOnce();
    }
}
