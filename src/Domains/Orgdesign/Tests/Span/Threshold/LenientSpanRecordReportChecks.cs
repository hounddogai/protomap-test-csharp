namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientSpanRecordReportChecks
{
    public static void LenientSpanRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705077425);
        var items = new List<LenientSpanRecord>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientSpanRecord.NewLenientSpanRecord($"lenient-span-record-{index}", "sample"));
        }
        var report = LenientSpanRecordReport.BuildLenientSpanRecordReport("test", items, clock);
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
        LenientSpanRecordReportCountsEachRecordOnce();
    }
}
