namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LicenseSeatOverrideReportChecks
{
    public static void LicenseSeatOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700841698);
        var items = new List<LicenseSeatOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LicenseSeatOverride.NewLicenseSeatOverride($"license-seat-override-{index}", "sample"));
        }
        var report = LicenseSeatOverrideReport.BuildLicenseSeatOverrideReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LicenseSeatOverrideReportCountsEachRecordOnce();
    }
}
