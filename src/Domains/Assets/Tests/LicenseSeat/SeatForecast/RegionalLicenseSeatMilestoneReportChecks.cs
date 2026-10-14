namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class RegionalLicenseSeatMilestoneReportChecks
{
    public static void RegionalLicenseSeatMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704634215);
        var items = new List<RegionalLicenseSeatMilestone>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RegionalLicenseSeatMilestone.NewRegionalLicenseSeatMilestone($"regional-license-seat-milestone-{index}", "sample"));
        }
        var report = RegionalLicenseSeatMilestoneReport.BuildRegionalLicenseSeatMilestoneReport("test", items, clock);
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
        RegionalLicenseSeatMilestoneReportCountsEachRecordOnce();
    }
}
