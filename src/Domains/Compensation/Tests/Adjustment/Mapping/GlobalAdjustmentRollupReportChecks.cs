namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalAdjustmentRollupReportChecks
{
    public static void GlobalAdjustmentRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705090333);
        var items = new List<GlobalAdjustmentRollup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GlobalAdjustmentRollup.NewGlobalAdjustmentRollup($"global-adjustment-rollup-{index}", "sample"));
        }
        var report = GlobalAdjustmentRollupReport.BuildGlobalAdjustmentRollupReport("test", items, clock);
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
        GlobalAdjustmentRollupReportCountsEachRecordOnce();
    }
}
