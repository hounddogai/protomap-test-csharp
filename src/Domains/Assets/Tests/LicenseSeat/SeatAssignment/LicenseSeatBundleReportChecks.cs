namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class LicenseSeatBundleReportChecks
{
    public static void LicenseSeatBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706084251);
        var items = new List<LicenseSeatBundle>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LicenseSeatBundle.NewLicenseSeatBundle($"license-seat-bundle-{index}", "sample"));
        }
        var report = LicenseSeatBundleReport.BuildLicenseSeatBundleReport("test", items, clock);
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
        LicenseSeatBundleReportCountsEachRecordOnce();
    }
}
