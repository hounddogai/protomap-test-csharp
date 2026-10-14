namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class BatchedRefreshAssignmentReportChecks
{
    public static void BatchedRefreshAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700801394);
        var items = new List<BatchedRefreshAssignment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BatchedRefreshAssignment.NewBatchedRefreshAssignment($"batched-refresh-assignment-{index}", "sample"));
        }
        var report = BatchedRefreshAssignmentReport.BuildBatchedRefreshAssignmentReport("test", items, clock);
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
        BatchedRefreshAssignmentReportCountsEachRecordOnce();
    }
}
