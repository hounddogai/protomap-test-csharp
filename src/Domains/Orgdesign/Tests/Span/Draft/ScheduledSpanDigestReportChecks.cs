namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledSpanDigestReportChecks
{
    public static void ScheduledSpanDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708978359);
        var items = new List<ScheduledSpanDigest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledSpanDigest.NewScheduledSpanDigest($"scheduled-span-digest-{index}", "sample"));
        }
        var report = ScheduledSpanDigestReport.BuildScheduledSpanDigestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScheduledSpanDigestReportCountsEachRecordOnce();
    }
}
