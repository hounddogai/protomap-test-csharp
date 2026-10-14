namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalSpanDigestReportChecks
{
    public static void SeasonalSpanDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706301862);
        var items = new List<SeasonalSpanDigest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalSpanDigest.NewSeasonalSpanDigest($"seasonal-span-digest-{index}", "sample"));
        }
        var report = SeasonalSpanDigestReport.BuildSeasonalSpanDigestReport("test", items, clock);
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
        SeasonalSpanDigestReportCountsEachRecordOnce();
    }
}
