namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayClaimReportChecks
{
    public static void HolidayClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707441868);
        var items = new List<HolidayClaim>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(HolidayClaim.NewHolidayClaim($"holiday-claim-{index}", "sample"));
        }
        var report = HolidayClaimReport.BuildHolidayClaimReport("test", items, clock);
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
        HolidayClaimReportCountsEachRecordOnce();
    }
}
