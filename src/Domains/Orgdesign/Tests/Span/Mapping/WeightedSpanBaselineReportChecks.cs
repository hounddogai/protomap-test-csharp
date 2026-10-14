namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class WeightedSpanBaselineReportChecks
{
    public static void WeightedSpanBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705269098);
        var items = new List<WeightedSpanBaseline>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedSpanBaseline.NewWeightedSpanBaseline($"weighted-span-baseline-{index}", "sample"));
        }
        var report = WeightedSpanBaselineReport.BuildWeightedSpanBaselineReport("test", items, clock);
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
        WeightedSpanBaselineReportCountsEachRecordOnce();
    }
}
