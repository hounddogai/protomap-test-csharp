namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatReservation;

using Acme.Workforce.Assets.LicenseSeat.SeatReservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of seat reservation code, run by the project's runner.</summary>
public static class LicenseSeatWindowReportChecks
{
    public static void LicenseSeatWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703659483);
        var items = new List<LicenseSeatWindow>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LicenseSeatWindow.NewLicenseSeatWindow($"license-seat-window-{index}", "sample"));
        }
        var report = LicenseSeatWindowReport.BuildLicenseSeatWindowReport("test", items, clock);
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
        LicenseSeatWindowReportCountsEachRecordOnce();
    }
}
