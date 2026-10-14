namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class GlobalSpanAdjustmentReportChecks
{
    public static void GlobalSpanAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701175617);
        var items = new List<GlobalSpanAdjustment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GlobalSpanAdjustment.NewGlobalSpanAdjustment($"global-span-adjustment-{index}", "sample"));
        }
        var report = GlobalSpanAdjustmentReport.BuildGlobalSpanAdjustmentReport("test", items, clock);
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
        GlobalSpanAdjustmentReportCountsEachRecordOnce();
    }
}
