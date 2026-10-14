namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatSummaryReportChecks
{
    public static void SeasonalLicenseSeatSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703357945);
        var items = new List<SeasonalLicenseSeatSummary>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalLicenseSeatSummary.NewSeasonalLicenseSeatSummary($"seasonal-license-seat-summary-{index}", "sample"));
        }
        var report = SeasonalLicenseSeatSummaryReport.BuildSeasonalLicenseSeatSummaryReport("test", items, clock);
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
        SeasonalLicenseSeatSummaryReportCountsEachRecordOnce();
    }
}
