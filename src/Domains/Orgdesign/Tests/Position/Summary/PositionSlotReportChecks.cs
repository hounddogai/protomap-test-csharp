namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PositionSlotReportChecks
{
    public static void PositionSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700370972);
        var items = new List<PositionSlot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PositionSlot.NewPositionSlot($"position-slot-{index}", "sample"));
        }
        var report = PositionSlotReport.BuildPositionSlotReport("test", items, clock);
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
        PositionSlotReportCountsEachRecordOnce();
    }
}
