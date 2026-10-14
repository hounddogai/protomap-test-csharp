namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class LicenseSeatEstimateReportChecks
{
    public static void LicenseSeatEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707011690);
        var items = new List<LicenseSeatEstimate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LicenseSeatEstimate.NewLicenseSeatEstimate($"license-seat-estimate-{index}", "sample"));
        }
        var report = LicenseSeatEstimateReport.BuildLicenseSeatEstimateReport("test", items, clock);
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
        LicenseSeatEstimateReportCountsEachRecordOnce();
    }
}
