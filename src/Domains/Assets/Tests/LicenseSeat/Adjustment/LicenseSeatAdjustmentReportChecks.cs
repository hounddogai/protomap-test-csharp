namespace Acme.Workforce.Assets.Tests.LicenseSeat.Adjustment;

using Acme.Workforce.Assets.LicenseSeat.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LicenseSeatAdjustmentReportChecks
{
    public static void LicenseSeatAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709988303);
        var items = new List<LicenseSeatAdjustment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LicenseSeatAdjustment.NewLicenseSeatAdjustment($"license-seat-adjustment-{index}", "sample"));
        }
        var report = LicenseSeatAdjustmentReport.BuildLicenseSeatAdjustmentReport("test", items, clock);
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
        LicenseSeatAdjustmentReportCountsEachRecordOnce();
    }
}
