namespace Acme.Workforce.Orgdesign.Tests.Span.SpanDraft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.SpanDraft;

/// <summary>Checks of span draft code, run by the project's runner.</summary>
public static class GlobalSpanDraftReportChecks
{
    public static void GlobalSpanDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700116249);
        var items = new List<GlobalSpanDraft>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GlobalSpanDraft.NewGlobalSpanDraft($"global-span-draft-{index}", "sample"));
        }
        var report = GlobalSpanDraftReport.BuildGlobalSpanDraftReport("test", items, clock);
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
        GlobalSpanDraftReportCountsEachRecordOnce();
    }
}
