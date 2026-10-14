namespace Acme.Workforce.Workplace.Tests.Booking.Group;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class CappedBookingTargetReportChecks
{
    public static void CappedBookingTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706191471);
        var items = new List<CappedBookingTarget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedBookingTarget.NewCappedBookingTarget($"capped-booking-target-{index}", "sample"));
        }
        var report = CappedBookingTargetReport.BuildCappedBookingTargetReport("test", items, clock);
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
        CappedBookingTargetReportCountsEachRecordOnce();
    }
}
