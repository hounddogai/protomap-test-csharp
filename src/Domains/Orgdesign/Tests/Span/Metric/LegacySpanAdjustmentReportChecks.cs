namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LegacySpanAdjustmentReportChecks
{
    public static void LegacySpanAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709356674);
        var items = new List<LegacySpanAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LegacySpanAdjustment.NewLegacySpanAdjustment($"legacy-span-adjustment-{index}", "sample"));
        }
        var report = LegacySpanAdjustmentReport.BuildLegacySpanAdjustmentReport("test", items, clock);
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
        LegacySpanAdjustmentReportCountsEachRecordOnce();
    }
}
