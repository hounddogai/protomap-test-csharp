namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class EngagementSlotReportChecks
{
    public static void EngagementSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703549875);
        var items = new List<EngagementSlot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(EngagementSlot.NewEngagementSlot($"engagement-slot-{index}", "sample"));
        }
        var report = EngagementSlotReport.BuildEngagementSlotReport("test", items, clock);
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
        EngagementSlotReportCountsEachRecordOnce();
    }
}
