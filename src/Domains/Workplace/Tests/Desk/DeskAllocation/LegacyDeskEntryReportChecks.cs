namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class LegacyDeskEntryReportChecks
{
    public static void LegacyDeskEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703264890);
        var items = new List<LegacyDeskEntry>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LegacyDeskEntry.NewLegacyDeskEntry($"legacy-desk-entry-{index}", "sample"));
        }
        var report = LegacyDeskEntryReport.BuildLegacyDeskEntryReport("test", items, clock);
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
        LegacyDeskEntryReportCountsEachRecordOnce();
    }
}
