namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class HeadcountSlotReportChecks
{
    public static void HeadcountSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703142100);
        var items = new List<HeadcountSlot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(HeadcountSlot.NewHeadcountSlot($"headcount-slot-{index}", "sample"));
        }
        var report = HeadcountSlotReport.BuildHeadcountSlotReport("test", items, clock);
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
        HeadcountSlotReportCountsEachRecordOnce();
    }
}
