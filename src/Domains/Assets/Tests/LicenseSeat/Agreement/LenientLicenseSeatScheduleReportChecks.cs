namespace Acme.Workforce.Assets.Tests.LicenseSeat.Agreement;

using Acme.Workforce.Assets.LicenseSeat.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientLicenseSeatScheduleReportChecks
{
    public static void LenientLicenseSeatScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702654257);
        var items = new List<LenientLicenseSeatSchedule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule($"lenient-license-seat-schedule-{index}", "sample"));
        }
        var report = LenientLicenseSeatScheduleReport.BuildLenientLicenseSeatScheduleReport("test", items, clock);
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
        LenientLicenseSeatScheduleReportCountsEachRecordOnce();
    }
}
