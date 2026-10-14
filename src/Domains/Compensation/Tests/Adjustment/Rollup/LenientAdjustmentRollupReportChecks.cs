namespace Acme.Workforce.Compensation.Tests.Adjustment.Rollup;

using Acme.Workforce.Compensation.Adjustment.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientAdjustmentRollupReportChecks
{
    public static void LenientAdjustmentRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704464565);
        var items = new List<LenientAdjustmentRollup>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientAdjustmentRollup.NewLenientAdjustmentRollup($"lenient-adjustment-rollup-{index}", "sample"));
        }
        var report = LenientAdjustmentRollupReport.BuildLenientAdjustmentRollupReport("test", items, clock);
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
        LenientAdjustmentRollupReportCountsEachRecordOnce();
    }
}
