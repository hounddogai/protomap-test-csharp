namespace Acme.Workforce.Talent.Tests.Mobility.OverrideItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class WeightedMobilityRollupReportChecks
{
    public static void WeightedMobilityRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702594212);
        var items = new List<WeightedMobilityRollup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(WeightedMobilityRollup.NewWeightedMobilityRollup($"weighted-mobility-rollup-{index}", "sample"));
        }
        var report = WeightedMobilityRollupReport.BuildWeightedMobilityRollupReport("test", items, clock);
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
        WeightedMobilityRollupReportCountsEachRecordOnce();
    }
}
