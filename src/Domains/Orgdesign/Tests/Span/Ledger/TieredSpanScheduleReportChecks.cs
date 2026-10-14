namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TieredSpanScheduleReportChecks
{
    public static void TieredSpanScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709536084);
        var items = new List<TieredSpanSchedule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredSpanSchedule.NewTieredSpanSchedule($"tiered-span-schedule-{index}", "sample"));
        }
        var report = TieredSpanScheduleReport.BuildTieredSpanScheduleReport("test", items, clock);
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
        TieredSpanScheduleReportCountsEachRecordOnce();
    }
}
