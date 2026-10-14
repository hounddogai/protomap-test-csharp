namespace Acme.Workforce.Orgdesign.Tests.Position.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PositionEntryReportChecks
{
    public static void PositionEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707124638);
        var items = new List<PositionEntry>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PositionEntry.NewPositionEntry($"position-entry-{index}", "sample"));
        }
        var report = PositionEntryReport.BuildPositionEntryReport("test", items, clock);
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
        PositionEntryReportCountsEachRecordOnce();
    }
}
