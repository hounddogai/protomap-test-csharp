namespace Acme.Workforce.Workplace.Tests.Booking.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AdaptiveBookingProfileReportChecks
{
    public static void AdaptiveBookingProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701893951);
        var items = new List<AdaptiveBookingProfile>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveBookingProfile.NewAdaptiveBookingProfile($"adaptive-booking-profile-{index}", "sample"));
        }
        var report = AdaptiveBookingProfileReport.BuildAdaptiveBookingProfileReport("test", items, clock);
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
        AdaptiveBookingProfileReportCountsEachRecordOnce();
    }
}
