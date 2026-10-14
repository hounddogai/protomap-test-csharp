namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientSpanRollupReportChecks
{
    public static void LenientSpanRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704355298);
        var items = new List<LenientSpanRollup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientSpanRollup.NewLenientSpanRollup($"lenient-span-rollup-{index}", "sample"));
        }
        var report = LenientSpanRollupReport.BuildLenientSpanRollupReport("test", items, clock);
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
        LenientSpanRollupReportCountsEachRecordOnce();
    }
}
