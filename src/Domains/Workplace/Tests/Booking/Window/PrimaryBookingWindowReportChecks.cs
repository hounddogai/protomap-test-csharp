namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PrimaryBookingWindowReportChecks
{
    public static void PrimaryBookingWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702320797);
        var items = new List<PrimaryBookingWindow>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PrimaryBookingWindow.NewPrimaryBookingWindow($"primary-booking-window-{index}", "sample"));
        }
        var report = PrimaryBookingWindowReport.BuildPrimaryBookingWindowReport("test", items, clock);
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
        PrimaryBookingWindowReportCountsEachRecordOnce();
    }
}
