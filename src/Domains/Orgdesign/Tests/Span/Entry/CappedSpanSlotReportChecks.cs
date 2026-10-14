namespace Acme.Workforce.Orgdesign.Tests.Span.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CappedSpanSlotReportChecks
{
    public static void CappedSpanSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702257388);
        var items = new List<CappedSpanSlot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedSpanSlot.NewCappedSpanSlot($"capped-span-slot-{index}", "sample"));
        }
        var report = CappedSpanSlotReport.BuildCappedSpanSlotReport("test", items, clock);
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
        CappedSpanSlotReportCountsEachRecordOnce();
    }
}
