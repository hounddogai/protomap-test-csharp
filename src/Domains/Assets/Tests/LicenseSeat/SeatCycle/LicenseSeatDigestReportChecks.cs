namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatCycle;

using Acme.Workforce.Assets.LicenseSeat.SeatCycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of seat cycle code, run by the project's runner.</summary>
public static class LicenseSeatDigestReportChecks
{
    public static void LicenseSeatDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704652120);
        var items = new List<LicenseSeatDigest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LicenseSeatDigest.NewLicenseSeatDigest($"license-seat-digest-{index}", "sample"));
        }
        var report = LicenseSeatDigestReport.BuildLicenseSeatDigestReport("test", items, clock);
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
        LicenseSeatDigestReportCountsEachRecordOnce();
    }
}
