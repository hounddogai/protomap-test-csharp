namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingPoolSlotReportChecks
{
    public static void PendingPoolSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702001324);
        var items = new List<PendingPoolSlot>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingPoolSlot.NewPendingPoolSlot($"pending-pool-slot-{index}", "sample"));
        }
        var report = PendingPoolSlotReport.BuildPendingPoolSlotReport("test", items, clock);
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
        PendingPoolSlotReportCountsEachRecordOnce();
    }
}
