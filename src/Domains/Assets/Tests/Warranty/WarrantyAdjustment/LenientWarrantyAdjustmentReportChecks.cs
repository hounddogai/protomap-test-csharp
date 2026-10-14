namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class LenientWarrantyAdjustmentReportChecks
{
    public static void LenientWarrantyAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704575027);
        var items = new List<LenientWarrantyAdjustment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientWarrantyAdjustment.NewLenientWarrantyAdjustment($"lenient-warranty-adjustment-{index}", "sample"));
        }
        var report = LenientWarrantyAdjustmentReport.BuildLenientWarrantyAdjustmentReport("test", items, clock);
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
        LenientWarrantyAdjustmentReportCountsEachRecordOnce();
    }
}
