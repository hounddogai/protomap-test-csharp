namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CappedBookingPreferenceReportChecks
{
    public static void CappedBookingPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709693417);
        var items = new List<CappedBookingPreference>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedBookingPreference.NewCappedBookingPreference($"capped-booking-preference-{index}", "sample"));
        }
        var report = CappedBookingPreferenceReport.BuildCappedBookingPreferenceReport("test", items, clock);
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
        CappedBookingPreferenceReportCountsEachRecordOnce();
    }
}
