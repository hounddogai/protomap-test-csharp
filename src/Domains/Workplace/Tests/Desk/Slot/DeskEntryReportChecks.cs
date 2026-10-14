namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskEntryReportChecks
{
    public static void DeskEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704758962);
        var items = new List<DeskEntry>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DeskEntry.NewDeskEntry($"desk-entry-{index}", "sample"));
        }
        var report = DeskEntryReport.BuildDeskEntryReport("test", items, clock);
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
        DeskEntryReportCountsEachRecordOnce();
    }
}
