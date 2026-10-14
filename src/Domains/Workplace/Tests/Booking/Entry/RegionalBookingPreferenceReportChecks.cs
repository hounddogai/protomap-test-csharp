namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RegionalBookingPreferenceReportChecks
{
    public static void RegionalBookingPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703534178);
        var items = new List<RegionalBookingPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RegionalBookingPreference.NewRegionalBookingPreference($"regional-booking-preference-{index}", "sample"));
        }
        var report = RegionalBookingPreferenceReport.BuildRegionalBookingPreferenceReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RegionalBookingPreferenceReportCountsEachRecordOnce();
    }
}
