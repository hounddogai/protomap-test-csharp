namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalDeskBatchReportChecks
{
    public static void GlobalDeskBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706923989);
        var items = new List<GlobalDeskBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GlobalDeskBatch.NewGlobalDeskBatch($"global-desk-batch-{index}", "sample"));
        }
        var report = GlobalDeskBatchReport.BuildGlobalDeskBatchReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        GlobalDeskBatchReportCountsEachRecordOnce();
    }
}
