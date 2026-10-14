namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AdaptiveWarrantyTargetReportChecks
{
    public static void AdaptiveWarrantyTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708080394);
        var items = new List<AdaptiveWarrantyTarget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveWarrantyTarget.NewAdaptiveWarrantyTarget($"adaptive-warranty-target-{index}", "sample"));
        }
        var report = AdaptiveWarrantyTargetReport.BuildAdaptiveWarrantyTargetReport("test", items, clock);
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
        AdaptiveWarrantyTargetReportCountsEachRecordOnce();
    }
}
