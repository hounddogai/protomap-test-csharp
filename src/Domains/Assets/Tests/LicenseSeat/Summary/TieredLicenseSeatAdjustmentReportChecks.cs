namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TieredLicenseSeatAdjustmentReportChecks
{
    public static void TieredLicenseSeatAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705091133);
        var items = new List<TieredLicenseSeatAdjustment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TieredLicenseSeatAdjustment.NewTieredLicenseSeatAdjustment($"tiered-license-seat-adjustment-{index}", "sample"));
        }
        var report = TieredLicenseSeatAdjustmentReport.BuildTieredLicenseSeatAdjustmentReport("test", items, clock);
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
        TieredLicenseSeatAdjustmentReportCountsEachRecordOnce();
    }
}
