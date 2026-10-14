namespace Acme.Workforce.Orgdesign.Tests.Span.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalSpanEntryReportChecks
{
    public static void GlobalSpanEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706989534);
        var items = new List<GlobalSpanEntry>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(GlobalSpanEntry.NewGlobalSpanEntry($"global-span-entry-{index}", "sample"));
        }
        var report = GlobalSpanEntryReport.BuildGlobalSpanEntryReport("test", items, clock);
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
        GlobalSpanEntryReportCountsEachRecordOnce();
    }
}
