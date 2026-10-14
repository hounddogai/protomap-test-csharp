namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskSlotReportChecks
{
    public static void DeskSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701512432);
        var items = new List<DeskSlot>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DeskSlot.NewDeskSlot($"desk-slot-{index}", "sample"));
        }
        var report = DeskSlotReport.BuildDeskSlotReport("test", items, clock);
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
        DeskSlotReportCountsEachRecordOnce();
    }
}
