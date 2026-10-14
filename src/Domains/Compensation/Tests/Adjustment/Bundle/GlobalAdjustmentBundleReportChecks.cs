namespace Acme.Workforce.Compensation.Tests.Adjustment.Bundle;

using Acme.Workforce.Compensation.Adjustment.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class GlobalAdjustmentBundleReportChecks
{
    public static void GlobalAdjustmentBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704545661);
        var items = new List<GlobalAdjustmentBundle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(GlobalAdjustmentBundle.NewGlobalAdjustmentBundle($"global-adjustment-bundle-{index}", "sample"));
        }
        var report = GlobalAdjustmentBundleReport.BuildGlobalAdjustmentBundleReport("test", items, clock);
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
        GlobalAdjustmentBundleReportCountsEachRecordOnce();
    }
}
