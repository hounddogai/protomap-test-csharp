namespace Acme.Workforce.Compensation.Tests.MeritCycle.Batch;

using Acme.Workforce.Compensation.MeritCycle.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingMeritCycleBatchReportChecks
{
    public static void PendingMeritCycleBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704844644);
        var items = new List<PendingMeritCycleBatch>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingMeritCycleBatch.NewPendingMeritCycleBatch($"pending-merit-cycle-batch-{index}", "sample"));
        }
        var report = PendingMeritCycleBatchReport.BuildPendingMeritCycleBatchReport("test", items, clock);
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
        PendingMeritCycleBatchReportCountsEachRecordOnce();
    }
}
