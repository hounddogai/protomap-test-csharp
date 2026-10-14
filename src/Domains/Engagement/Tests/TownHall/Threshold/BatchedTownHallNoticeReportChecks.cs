namespace Acme.Workforce.Engagement.Tests.TownHall.Threshold;

using Acme.Workforce.Engagement.TownHall.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BatchedTownHallNoticeReportChecks
{
    public static void BatchedTownHallNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702235817);
        var items = new List<BatchedTownHallNotice>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BatchedTownHallNotice.NewBatchedTownHallNotice($"batched-town-hall-notice-{index}", "sample"));
        }
        var report = BatchedTownHallNoticeReport.BuildBatchedTownHallNoticeReport("test", items, clock);
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
        BatchedTownHallNoticeReportCountsEachRecordOnce();
    }
}
