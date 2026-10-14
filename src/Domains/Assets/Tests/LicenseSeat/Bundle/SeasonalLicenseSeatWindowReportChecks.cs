namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatWindowReportChecks
{
    public static void SeasonalLicenseSeatWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705113578);
        var items = new List<SeasonalLicenseSeatWindow>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow($"seasonal-license-seat-window-{index}", "sample"));
        }
        var report = SeasonalLicenseSeatWindowReport.BuildSeasonalLicenseSeatWindowReport("test", items, clock);
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
        SeasonalLicenseSeatWindowReportCountsEachRecordOnce();
    }
}
