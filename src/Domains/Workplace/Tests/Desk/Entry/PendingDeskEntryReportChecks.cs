namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PendingDeskEntryReportChecks
{
    public static void PendingDeskEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709236394);
        var items = new List<PendingDeskEntry>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingDeskEntry.NewPendingDeskEntry($"pending-desk-entry-{index}", "sample"));
        }
        var report = PendingDeskEntryReport.BuildPendingDeskEntryReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PendingDeskEntryReportCountsEachRecordOnce();
    }
}
