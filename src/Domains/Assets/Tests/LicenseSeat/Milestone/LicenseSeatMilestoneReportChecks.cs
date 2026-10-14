namespace Acme.Workforce.Assets.Tests.LicenseSeat.Milestone;

using Acme.Workforce.Assets.LicenseSeat.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LicenseSeatMilestoneReportChecks
{
    public static void LicenseSeatMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709233249);
        var items = new List<LicenseSeatMilestone>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LicenseSeatMilestone.NewLicenseSeatMilestone($"license-seat-milestone-{index}", "sample"));
        }
        var report = LicenseSeatMilestoneReport.BuildLicenseSeatMilestoneReport("test", items, clock);
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
        LicenseSeatMilestoneReportCountsEachRecordOnce();
    }
}
