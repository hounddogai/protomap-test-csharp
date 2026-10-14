namespace Acme.Workforce.Orgdesign.Tests.Reorg.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedReorgTargetReportChecks
{
    public static void WeightedReorgTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703606902);
        var items = new List<WeightedReorgTarget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedReorgTarget.NewWeightedReorgTarget($"weighted-reorg-target-{index}", "sample"));
        }
        var report = WeightedReorgTargetReport.BuildWeightedReorgTargetReport("test", items, clock);
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
        WeightedReorgTargetReportCountsEachRecordOnce();
    }
}
