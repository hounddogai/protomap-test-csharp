namespace Acme.Workforce.Workplace.Tests.Booking.BookingReview;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Checks of booking review code, run by the project's runner.</summary>
public static class ManualBookingPreferenceReportChecks
{
    public static void ManualBookingPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703434825);
        var items = new List<ManualBookingPreference>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ManualBookingPreference.NewManualBookingPreference($"manual-booking-preference-{index}", "sample"));
        }
        var report = ManualBookingPreferenceReport.BuildManualBookingPreferenceReport("test", items, clock);
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
        ManualBookingPreferenceReportCountsEachRecordOnce();
    }
}
